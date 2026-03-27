using System.Windows;
using DeliveryDesktop.Models;
using DeliveryDesktop.Services;

namespace DeliveryDesktop.Views;

public partial class AdminWindow : Window
{
    private readonly ApiService _api;

    public AdminWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;

        // Многоадресный делегат
        _api.RequestCompleted += LogToStatus;
        _api.RequestCompleted += (endpoint, statusCode, elapsedMs) =>
    _api.Log($"{endpoint} | {statusCode} | {elapsedMs}мс");

        Loaded += async (_, _) => await LoadAll();
    }

    private void LogToStatus(string endpoint, int statusCode, long elapsedMs)
    {
        Dispatcher.Invoke(() =>
            AdminLogText.Text = $"[{DateTime.Now:HH:mm:ss}] {endpoint} → HTTP {statusCode} ({elapsedMs} мс)");
    }

    private async Task LoadAll()
    {
        try
        {
            await LoadPackages();
            await LoadClients();
            await LoadCouriers();
            await LoadRoutes();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
        }
    }

    private async Task LoadPackages()
    {
        AdminPackagesGrid.ItemsSource = await _api.GetAllPackages();
    }

    private async Task LoadClients()
    {
        ClientsGrid.ItemsSource = await _api.GetAllClients();
    }

    private async Task LoadCouriers()
    {
        CouriersGrid.ItemsSource = await _api.GetAllCouriers();
    }

    private async Task LoadRoutes()
    {
        RoutesGrid.ItemsSource = await _api.GetAllRoutes();
    }

    private async void RefreshPackages_Click(object sender, RoutedEventArgs e)
        => await LoadPackages();

    private async void RefreshClients_Click(object sender, RoutedEventArgs e)
        => await LoadClients();

    private async void RefreshCouriers_Click(object sender, RoutedEventArgs e)
        => await LoadCouriers();

    private async void RefreshRoutes_Click(object sender, RoutedEventArgs e)
        => await LoadRoutes();

    private async void DeletePackage_Click(object sender, RoutedEventArgs e)
    {
        if (AdminPackagesGrid.SelectedItem is not Package pkg) return;

        var result = MessageBox.Show(
            $"Удалить посылку {pkg.TrackingNumber}?",
            "Подтверждение", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            await _api.DeletePackage(pkg.Id);
            await LoadPackages();
        }
    }

    private async void UpdateStatus_Click(object sender, RoutedEventArgs e)
    {
        if (AdminPackagesGrid.SelectedItem is not Package pkg) return;

        var statuses = new[] { "Pending", "InTransit", "Delivered" };
        var current = Array.IndexOf(statuses, pkg.Status);
        var next = statuses[(current + 1) % statuses.Length];

        pkg.Status = next;
        await _api.UpdatePackage(pkg.Id, pkg);
        await LoadPackages();
    }
}