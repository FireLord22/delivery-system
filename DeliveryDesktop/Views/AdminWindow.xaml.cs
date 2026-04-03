using System.Windows;
using System.Windows.Controls;
using DeliveryDesktop.Models;
using DeliveryDesktop.Services;

namespace DeliveryDesktop.Views;

public partial class AdminWindow : Window
{
    private readonly ApiService _api;
    private List<Route> _routes = new();

    private static readonly string[] Statuses = { "Pending", "InTransit", "Delivered" };
    private static readonly Dictionary<string, string> StatusRuMap = new()
    {
        { "Pending", "Ожидает" },
        { "InTransit", "В пути" },
        { "Delivered", "Доставлено" }
    };

    public AdminWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
        _api.RequestCompleted += LogToStatus;
        _api.RequestCompleted += (endpoint, statusCode, elapsedMs) =>
            _api.Log($"{endpoint} | {statusCode} | {elapsedMs}мс");

        Loaded += async (_, _) => await LoadAll();
    }

    private void LogToStatus(string endpoint, int statusCode, long elapsedMs)
    {
        Dispatcher.Invoke(() =>
            AdminLogText.Text =
                $"[{DateTime.Now:HH:mm:ss}] {endpoint} → HTTP {statusCode} ({elapsedMs} мс)");
    }

    private async Task LoadAll()
    {
        await LoadRoutes();
        await LoadPackages();
        await LoadClients();
        await LoadCouriers();
    }

    private async Task LoadPackages()
    {
        try
        {
            AdminPackagesGrid.ItemsSource = await _api.GetAllPackages();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Тип: {ex.GetType().Name}\n\nСообщение: {ex.Message}\n\nВнутренняя: {ex.InnerException?.Message}\n\nСтек: {ex.StackTrace}", "Ошибка");
        }
    }

    private async Task LoadClients()
    {
        try { ClientsGrid.ItemsSource = await _api.GetAllClients(); }
        catch { }
    }

    private async Task LoadCouriers()
    {
        try { CouriersGrid.ItemsSource = await _api.GetAllCouriers(); }
        catch { }
    }

    private async Task LoadRoutes()
    {
        try
        {
            _routes = await _api.GetAllRoutes();
            RoutesGrid.ItemsSource = _routes;
        }
        catch { }
    }

    // ── Выбор посылки ─────────────────────────────────────────────────────
    private void AdminPackagesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdminPackagesGrid.SelectedItem is Package pkg)
            PackageHint.Text =
                $"Выбрана: {pkg.TrackingNumber}  |  Статус: {StatusRuMap.GetValueOrDefault(pkg.Status, pkg.Status)}  |  Маршрут: {pkg.Route?.Name ?? $"ID {pkg.RouteId}"}";
        else
            PackageHint.Text = "Выберите посылку в таблице";
    }

    // ── Кнопки статуса ────────────────────────────────────────────────────
    private async void NextStatus_Click(object sender, RoutedEventArgs e)
        => await ShiftStatus(+1);

    private async void PrevStatus_Click(object sender, RoutedEventArgs e)
        => await ShiftStatus(-1);

    private async Task ShiftStatus(int direction)
    {
        if (AdminPackagesGrid.SelectedItem is not Package pkg)
        {
            MessageBox.Show("Выберите посылку в таблице");
            return;
        }

        var idx = Array.IndexOf(Statuses, pkg.Status);
        var newIdx = idx + direction;

        if (newIdx < 0 || newIdx >= Statuses.Length)
        {
            MessageBox.Show(direction > 0
                ? "Статус «Доставлено» — это последний статус"
                : "Статус «Ожидает» — это первый статус");
            return;
        }

        pkg.Status = Statuses[newIdx];
        await _api.UpdatePackage(pkg.Id, pkg);
        await LoadPackages();
    }

    // ── Смена маршрута ────────────────────────────────────────────────────
    private async void ChangeRoute_Click(object sender, RoutedEventArgs e)
    {
        if (AdminPackagesGrid.SelectedItem is not Package pkg)
        {
            MessageBox.Show("Выберите посылку в таблице");
            return;
        }

        if (pkg.Status != "Pending")
        {
            MessageBox.Show(
                "Маршрут можно менять только у посылок со статусом «Ожидает»",
                "Недоступно", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_routes.Count == 0) { MessageBox.Show("Список маршрутов не загружен"); return; }

        var dialog = new Window
        {
            Title = $"Смена маршрута — {pkg.TrackingNumber}",
            Width = 420,
            Height = 280,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var listBox = new ListBox { Margin = new Thickness(10, 10, 10, 0) };
        foreach (var r in _routes)
            listBox.Items.Add($"{r.Name}  ({r.StartPoint} → {r.EndPoint}, {r.DistanceKm} км)");

        var currentIdx = _routes.FindIndex(r => r.Id == pkg.RouteId);
        if (currentIdx >= 0) listBox.SelectedIndex = currentIdx;

        var btnApply = new Button
        {
            Content = "Применить",
            Margin = new Thickness(10),
            Padding = new Thickness(14, 5, 14, 5),
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var stack = new StackPanel();
        stack.Children.Add(listBox);
        stack.Children.Add(btnApply);
        dialog.Content = stack;

        btnApply.Click += async (_, _) =>
        {
            if (listBox.SelectedIndex < 0) { MessageBox.Show("Выберите маршрут"); return; }
            pkg.RouteId = _routes[listBox.SelectedIndex].Id;
            await _api.UpdatePackage(pkg.Id, pkg);
            await LoadPackages();
            dialog.Close();
        };

        dialog.ShowDialog();
    }

    // ── Удаление посылки ──────────────────────────────────────────────────
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

    // ── КЛИЕНТЫ ───────────────────────────────────────────────────────────
    private async void AddClient_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ClientNameBox.Text) ||
            string.IsNullOrWhiteSpace(ClientPhoneBox.Text))
        {
            MessageBox.Show("Заполните имя и телефон");
            return;
        }

        var client = new Client
        {
            FullName = ClientNameBox.Text.Trim(),
            Phone = ClientPhoneBox.Text.Trim(),
            Email = ClientEmailBox.Text.Trim(),
            Address = ClientAddressBox.Text.Trim()
        };

        var ok = await _api.CreateClient(client);
        if (ok)
        {
            ClientNameBox.Clear();
            ClientPhoneBox.Clear();
            ClientEmailBox.Clear();
            ClientAddressBox.Clear();
            await LoadClients();
        }
        else
        {
            MessageBox.Show("Ошибка при добавлении клиента");
        }
    }

    private async void DeleteClient_Click(object sender, RoutedEventArgs e)
    {
        if (ClientsGrid.SelectedItem is not Client client) return;

        var result = MessageBox.Show(
            $"Удалить клиента {client.FullName}?",
            "Подтверждение", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            await _api.DeleteClient(client.Id);
            await LoadClients();
        }
    }

    private async void RefreshClients_Click(object sender, RoutedEventArgs e)
        => await LoadClients();

    // ── КУРЬЕРЫ ───────────────────────────────────────────────────────────
    private async void AddCourier_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CourierNameBox.Text) ||
            string.IsNullOrWhiteSpace(CourierPhoneBox.Text))
        {
            MessageBox.Show("Заполните имя и телефон");
            return;
        }

        var courier = new Courier
        {
            FullName = CourierNameBox.Text.Trim(),
            Phone = CourierPhoneBox.Text.Trim(),
            Vehicle = CourierVehicleBox.Text.Trim(),
            IsAvailable = CourierAvailableBox.IsChecked ?? true
        };

        var ok = await _api.CreateCourier(courier);
        if (ok)
        {
            CourierNameBox.Clear();
            CourierPhoneBox.Clear();
            CourierVehicleBox.Clear();
            CourierAvailableBox.IsChecked = true;
            await LoadCouriers();
        }
        else
        {
            MessageBox.Show("Ошибка при добавлении курьера");
        }
    }

    private async void DeleteCourier_Click(object sender, RoutedEventArgs e)
    {
        if (CouriersGrid.SelectedItem is not Courier courier) return;

        var result = MessageBox.Show(
            $"Удалить курьера {courier.FullName}?",
            "Подтверждение", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            await _api.DeleteCourier(courier.Id);
            await LoadCouriers();
        }
    }

    private async void RefreshCouriers_Click(object sender, RoutedEventArgs e)
        => await LoadCouriers();

    // ── МАРШРУТЫ ──────────────────────────────────────────────────────────
    private async void AddRoute_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RouteNameBox.Text) ||
            string.IsNullOrWhiteSpace(RouteStartBox.Text) ||
            string.IsNullOrWhiteSpace(RouteEndBox.Text))
        {
            MessageBox.Show("Заполните название, откуда и куда");
            return;
        }

        if (!double.TryParse(RouteDistanceBox.Text, out var distance))
        {
            MessageBox.Show("Расстояние должно быть числом");
            return;
        }

        var route = new Route
        {
            Name = RouteNameBox.Text.Trim(),
            StartPoint = RouteStartBox.Text.Trim(),
            EndPoint = RouteEndBox.Text.Trim(),
            DistanceKm = distance
        };

        var ok = await _api.CreateRoute(route);
        if (ok)
        {
            RouteNameBox.Clear();
            RouteStartBox.Clear();
            RouteEndBox.Clear();
            RouteDistanceBox.Clear();
            await LoadRoutes();
        }
        else
        {
            MessageBox.Show("Ошибка при добавлении маршрута");
        }
    }

    private async void DeleteRoute_Click(object sender, RoutedEventArgs e)
    {
        if (RoutesGrid.SelectedItem is not Route route) return;

        var result = MessageBox.Show(
            $"Удалить маршрут {route.Name}?",
            "Подтверждение", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            await _api.DeleteRoute(route.Id);
            await LoadRoutes();
        }
    }

    private async void RefreshRoutes_Click(object sender, RoutedEventArgs e)
        => await LoadRoutes();

    private async void RefreshPackages_Click(object sender, RoutedEventArgs e)
        => await LoadPackages();
}
