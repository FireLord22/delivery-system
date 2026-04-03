using System.Windows;
using System.Windows.Media;
using DeliveryDesktop.Models;
using DeliveryDesktop.Services;

namespace DeliveryDesktop.Views;

public partial class UserWindow : Window
{
    private readonly ApiService _api;

    private static readonly Dictionary<string, string> StatusTranslations = new()
    {
        { "Pending", "Ожидает" },
        { "InTransit", "В пути" },
        { "Delivered", "Доставлено" }
    };

    public UserWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
        _api.RequestCompleted += LogToStatus;
        _api.RequestCompleted += (endpoint, statusCode, elapsedMs) =>
            _api.Log($"{endpoint} | {statusCode} | {elapsedMs}мс");
    }

    private void LogToStatus(string endpoint, int statusCode, long elapsedMs)
    {
        Dispatcher.Invoke(() =>
            LogText.Text = $"[{DateTime.Now:HH:mm:ss}] {endpoint} → HTTP {statusCode} ({elapsedMs} мс)");
    }

    private async void SearchPackage_Click(object sender, RoutedEventArgs e)
    {
        var trackingNumber = SearchBox.Text.Trim();
        if (string.IsNullOrEmpty(trackingNumber))
        {
            MessageBox.Show("Введите трек-номер для поиска");
            return;
        }

        // Ищем среди всех посылок
        var packages = await _api.GetAllPackages();
        var package = packages.FirstOrDefault(p =>
            p.TrackingNumber.Equals(trackingNumber, StringComparison.OrdinalIgnoreCase));

        if (package == null)
        {
            SearchResultTitle.Text = $"Посылка с трек-номером «{trackingNumber}» не найдена";
            SearchResultTitle.Foreground = Brushes.Red;
            PackageInfoGrid.Visibility = System.Windows.Visibility.Collapsed;
            return;
        }

        // Показываем информацию
        SearchResultTitle.Text = $"Информация о посылке:";
        SearchResultTitle.Foreground = Brushes.Black;
        PackageInfoGrid.Visibility = System.Windows.Visibility.Visible;

        InfoTracking.Text = package.TrackingNumber;
        InfoWeight.Text = $"{package.WeightKg} кг";
        InfoClient.Text = package.Client?.FullName ?? $"ID {package.ClientId}";
        InfoCourier.Text = package.Courier?.FullName ?? $"ID {package.CourierId}";
        InfoRoute.Text = package.Route?.Name ?? $"ID {package.RouteId}";

        // Статус с цветом
        var statusRu = StatusTranslations.TryGetValue(package.Status, out var translated)
            ? translated : package.Status;
        InfoStatus.Text = statusRu;
        InfoStatus.Foreground = package.Status switch
        {
            "Delivered" => Brushes.Green,
            "InTransit" => Brushes.Blue,
            _ => Brushes.Orange
        };
    }

    private async void CreatePackage_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(WeightBox.Text, out var weight) ||
            !int.TryParse(ClientIdBox.Text, out var clientId))
        {
            MessageBox.Show("Проверьте введённые данные");
            return;
        }

        // Генерируем трек-номер автоматически
        var trackingNumber = $"TRK-{DateTime.Now:yyyyMMdd-HHmmss}";

        var package = new Package
        {
            TrackingNumber = trackingNumber,
            WeightKg = weight,
            Status = "Pending",
            ClientId = clientId,
            CourierId = 1,
            RouteId = 1
        };

        var ok = await _api.CreatePackage(package);
        if (ok)
        {
            WeightBox.Clear();
            ClientIdBox.Clear();
            MessageBox.Show($"Посылка создана!\nТрек-номер: {trackingNumber}",
                "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Ошибка при создании посылки");
        }
    }
}