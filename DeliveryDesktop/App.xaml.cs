using System.Windows;
using DeliveryDesktop.Views;

namespace DeliveryDesktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new LoginWindow().Show();
    }
}