using System.Windows;

namespace MouseStudio;

public partial class ProfileNameDialog : Window
{
    // Returns an error message for an unusable name, or null to accept it.
    private readonly Func<string, string?> _validate;

    public string ProfileName => NameTextBox.Text.Trim();

    public ProfileNameDialog(
        Window owner,
        string title,
        string initialName,
        Func<string, string?> validate
    )
    {
        InitializeComponent();

        Owner = owner;
        Title = title;
        _validate = validate;

        NameTextBox.Text = initialName;

        Loaded += (_, _) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }

    private void Ok_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        var error = _validate(NameTextBox.Text);

        if (error != null)
        {
            ErrorText.Text = error;
            ErrorText.Visibility = Visibility.Visible;

            return;
        }

        DialogResult = true;
    }
}
