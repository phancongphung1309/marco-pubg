using System.Windows.Data;
using System.Windows.Markup;

namespace MouseStudio.Core.Localization;

// {loc:Tr Key} in XAML: the text of `Key` in the current language,
// updated when the language changes.
[MarkupExtensionReturnType(typeof(object))]
public class TrExtension : MarkupExtension
{
    public string Key { get; set; }

    public TrExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay
        }.ProvideValue(serviceProvider);
}
