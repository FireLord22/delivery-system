using System.Windows;
using DeliveryDesktop.Models;
using DeliveryDesktop.Services;

namespace DeliveryDesktop.Views;

public partial class UserWindow : Window
{
    private readonly ApiService _api;

    public UserWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;

        // Подписываем обработчики делегата
        _api.RequestCompleted += LogToStatus;
        _api.RequestCompleted += (endpoint, statusCode, elapsedMs) =>
     _api.Log($"{endpoint} | {statusCode} | {elapsedMs}мс");

        Loaded += async (_, _) => await LoadPackages();
    }

    private void LogToStatus(string endpoint, int statusCode, long elapsedMs)
    {
        Dispatcher.Invoke(() =>
            LogText.Text = $"[{DateTime.Now:HH:mm:ss}] {endpoint} → HTTP {statusCode} ({elapsedMs} мс)");
    }

    private async Task LoadPackages()
    {
        try
        {
            var packages = await _api.GetAllPackages();
            PackagesGrid.ItemsSource = packages;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Тип: {ex.GetType().Name}\n\nСообщение: {ex.Message}\n\nВнутренняя: {ex.InnerException?.Message}", "Ошибка");
        }
    }

    private async void CreatePackage_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(WeightBox.Text, out var weight) ||
            !int.TryParse(ClientIdBox.Text, out var clientId))
        {
            MessageBox.Show("Проверьте введённые данные");
            return;
        }

        var package = new Package
        {
            TrackingNumber = TrackingBox.Text,
            WeightKg = weight,
            Status = "Pending",
            ClientId = clientId,
            CourierId = 1,
            RouteId = 1
        };

        var ok = await _api.CreatePackage(package);
        if (ok)
        {
            TrackingBox.Clear();
            WeightBox.Clear();
            ClientIdBox.Clear();
            await LoadPackages();
        }
        else
        {
            MessageBox.Show("Ошибка при создании посылки");
        }
    }
}