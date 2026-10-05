using System.ComponentModel;
using System.Globalization;

namespace MouseStudio.Core.Localization;

// The UI language and its strings. XAML binds to the indexer (see
// TrExtension), so every bound text refreshes when the language changes;
// code calls Loc.T and re-applies its texts on LanguageChanged.
public sealed class Loc : INotifyPropertyChanged
{
    public const string English = "en";
    public const string Vietnamese = "vi";

    public static Loc Instance { get; } = new();

    public string Language { get; private set; } = English;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Raised after the bindings have been told to refresh.
    public event Action? LanguageChanged;

    private Loc()
    {
    }

    // Missing translations fall back to English, then to the key itself.
    public string this[string key] =>
        Strings.Get(Language, key)
        ?? Strings.Get(English, key)
        ?? key;

    public static string T(string key, params object?[] args)
    {
        var text = Instance[key];

        return args.Length == 0
            ? text
            : string.Format(CultureInfo.InvariantCulture, text, args);
    }

    // Unknown or missing codes select English.
    public void SetLanguage(string? language)
    {
        language = language == Vietnamese ? Vietnamese : English;

        if (language == Language)
        {
            return;
        }

        Language = language;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));

        LanguageChanged?.Invoke();
    }
}
