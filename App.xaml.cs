using System.IO;
using System.Windows;
using System.Windows.Threading;
using MouseStudio.Core.Localization;

namespace MouseStudio;

public partial class App : Application
{
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MouseStudio",
        "crash.log"
    );

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            WriteCrashLog("UnobservedTaskException", e.Exception);

            e.SetObserved();
        };
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e
    )
    {
        WriteCrashLog("DispatcherUnhandledException", e.Exception);

        MessageBox.Show(
            Loc.T("UnexpectedError", e.Exception.Message, CrashLogPath),
            "Mouse Studio",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );

        e.Handled = true;
    }

    private static void OnUnhandledException(
        object sender,
        UnhandledExceptionEventArgs e
    )
    {
        WriteCrashLog("UnhandledException", e.ExceptionObject as Exception);
    }

    private static void WriteCrashLog(string source, Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CrashLogPath)!);

            File.AppendAllText(
                CrashLogPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}"
            );
        }
        catch
        {
            // Never let crash logging itself crash the app.
        }
    }
}
