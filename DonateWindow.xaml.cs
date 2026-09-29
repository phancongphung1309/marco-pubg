using System.Windows;

namespace MouseStudio;

// Shows the donation QR code.
public partial class DonateWindow : Window
{
    public DonateWindow(Window owner)
    {
        InitializeComponent();

        Owner = owner;
    }
}
