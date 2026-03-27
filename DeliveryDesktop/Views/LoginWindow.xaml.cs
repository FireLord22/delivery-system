using System.Windows;
using DeliveryDesktop.Services;

namespace DeliveryDesktop.Views;

public partial class LoginWindow : Window
{
    // Жёстко заданные пользователи (в реальном проекте — из БД)
    private readonly Dictionary<string, (string password, string role)> _users = new()
    {
        { "admin", ("admin123", "admin") },
        { "user",  ("user123",  "user")  }
    };

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (!_users.TryGetValue(username, out var info))
        {
            ErrorText.Text = "Пользователь не найден";
            return;
        }

        if (info.password != password)
        {
            ErrorText.Text = "Неверный пароль";
            return;
        }

        var api = new ApiService();

        if (info.role == "admin")
        {
            new AdminWindow(api).Show();
        }
        else
        {
            new UserWindow(api).Show();
        }

        Close();
    }
}