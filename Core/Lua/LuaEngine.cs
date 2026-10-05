using System.Collections.Concurrent;
using NLua;
using LuaRuntime = NLua.Lua;
using MouseStudio.Core.Input;

namespace MouseStudio.Core.Lua;

// A Lua state is not thread-safe, so every access to it (load, OnEvent, dispose)
// runs on one dedicated worker thread, fed by a queue.
public class LuaEngine : IDisposable
{
    private sealed class Session : IDisposable
    {
        public required LuaRuntime Lua { get; init; }

        public required CancellationTokenSource Cancellation { get; init; }

        public void Dispose()
        {
            Lua.Dispose();

            Cancellation.Dispose();
        }
    }

    private readonly BlockingCollection<Action> _queue = new();

    private readonly Thread _worker;

    // Owned by the worker thread: the Lua state that is currently loaded.
    private Session? _loaded;

    // Written by the UI thread: the session that events should be sent to.
    private volatile Session? _active;

    private bool _disposed;

    public bool IsRunning => _active != null;

    public event Action<string>? LogReceived;

    public LuaEngine()
    {
        _worker = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = "Lua Worker"
        };

        _worker.Start();
    }

    // `globals` are set before the script runs (e.g. MOVE_X = 5).
    public void Start(
        string scriptPath,
        IReadOnlyDictionary<string, object> globals
    )
    {
        Stop();

        var cancellation = new CancellationTokenSource();

        var session = new Session
        {
            Lua = new LuaRuntime(),
            Cancellation = cancellation
        };

        _active = session;

        Enqueue(() =>
        {
            UnloadCurrent();

            _loaded = session;

            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var api = new LuaApi(Log, cancellation.Token);

                RegisterApi(session.Lua, api);

                foreach (var (name, value) in globals)
                {
                    session.Lua[name] = value;
                }

                session.Lua.DoFile(scriptPath);
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");

                cancellation.Cancel();

                if (_active == session)
                {
                    _active = null;
                }

                return;
            }

            Log($"Loaded: {scriptPath}");

            Log(
                "Config: " +
                string.Join(", ", globals.Select(g => $"{g.Key}={g.Value}"))
            );

            CallOnEvent(session, "PROFILE_ACTIVATED", 0);
        });
    }

    private static void RegisterApi(LuaRuntime lua, LuaApi api)
    {
        var type = api.GetType();

        lua.RegisterFunction("MoveMouseRelative", api, type.GetMethod(nameof(LuaApi.MoveMouseRelative)));
        lua.RegisterFunction("Sleep", api, type.GetMethod(nameof(LuaApi.Sleep)));
        lua.RegisterFunction("OutputLogMessage", api, type.GetMethod(nameof(LuaApi.OutputLogMessage)));
        lua.RegisterFunction("ClickMouseButton", api, type.GetMethod(nameof(LuaApi.ClickMouseButton)));
        lua.RegisterFunction("IsMouseButtonPressed", api, type.GetMethod(nameof(LuaApi.IsMouseButtonPressed)));
        lua.RegisterFunction("IsHotkeyPressed", api, type.GetMethod(nameof(LuaApi.IsHotkeyPressed)));
        lua.RegisterFunction("PressMouseButton", api, type.GetMethod(nameof(LuaApi.PressMouseButton)));
        lua.RegisterFunction("ReleaseMouseButton", api, type.GetMethod(nameof(LuaApi.ReleaseMouseButton)));
        lua.RegisterFunction("GetTickCount", api, type.GetMethod(nameof(LuaApi.GetTickCount)));
        lua.RegisterFunction("IsKeyLockOn", api, type.GetMethod(nameof(LuaApi.IsKeyLockOn)));
    }

    // Safe to call from any thread: the event is queued for the worker.
    public void Dispatch(
        string eventName,
        int arg
    )
    {
        var session = _active;

        if (session == null)
        {
            return;
        }

        Enqueue(() => CallOnEvent(session, eventName, arg));
    }

    private void CallOnEvent(
        Session session,
        string eventName,
        int arg
    )
    {
        // Skip events queued for a script that has since been stopped or replaced.
        if (_loaded != session || session.Cancellation.IsCancellationRequested)
        {
            return;
        }

        try
        {
            using var function =
                session.Lua.GetFunction("OnEvent");

            function?.Call(eventName, arg);
        }
        catch (Exception ex)
        {
            if (session.Cancellation.IsCancellationRequested)
            {
                return;
            }

            Log($"Lua Error: {ex.Message}");
        }
    }

    private void Log(string message)
    {
        LogReceived?.Invoke(message);
    }

    // Interrupts the running script (at its next API call, e.g. Sleep)
    // and releases the Lua state on the worker thread.
    public void Stop()
    {
        var session = _active;

        _active = null;

        if (session == null)
        {
            return;
        }

        session.Cancellation.Cancel();

        Enqueue(UnloadCurrent);
    }

    private void UnloadCurrent()
    {
        _loaded?.Dispose();

        _loaded = null;
    }

    private void Enqueue(Action action)
    {
        try
        {
            _queue.Add(action);
        }
        catch (InvalidOperationException)
        {
            // Engine is disposed; ignore late events.
        }
    }

    private void WorkerLoop()
    {
        foreach (var action in _queue.GetConsumingEnumerable())
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log($"Lua Engine Error: {ex.Message}");
            }
        }

        UnloadCurrent();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Stop();

        _queue.CompleteAdding();

        // A script stuck in a loop that never calls the API can't be interrupted;
        // don't hang app shutdown on it (the worker is a background thread).
        _worker.Join(TimeSpan.FromSeconds(2));
    }
}
