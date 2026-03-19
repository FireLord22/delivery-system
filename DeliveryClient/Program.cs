using System.Diagnostics;
using System.Net.Http.Json;

var api = new ApiService();
api.RequestCompleted += ConsoleHandler;
api.RequestCompleted += LogFileHandler;

Console.WriteLine("=== Delivery System Client ===\n");

Console.WriteLine("1. Создание посылки...");
await api.CreatePackage(new { TrackingNumber = "TRK999", WeightKg = 1.5, Status = "Pending", ClientId = 1, CourierId = 1, RouteId = 1 });

Console.WriteLine("\n2. Получение всех посылок...");
var all = await api.GetAllPackages();
Console.WriteLine("   Получено: " + all[..Math.Min(100, all.Length)] + "...");

Console.WriteLine("\n3. Получение посылки ID=1...");
var one = await api.GetById(1);
Console.WriteLine("   Результат: " + one?[..Math.Min(80, one?.Length ?? 0)]);

Console.WriteLine("\n>>> Отписываем FileHandler <<<\n");
api.RequestCompleted -= LogFileHandler;

Console.WriteLine("4. Обновление посылки ID=1...");
await api.UpdatePackage(1, new { TrackingNumber = "TRK001", WeightKg = 2.5, Status = "Delivered", ClientId = 1, CourierId = 1, RouteId = 1 });

Console.WriteLine("\n5. Удаление посылки...");
await api.DeletePackage(3);

Console.WriteLine("\n=== Готово! Проверьте api_log.txt ===");

void ConsoleHandler(string endpoint, int statusCode, long elapsedMs)
    => Console.WriteLine("[CONSOLE] " + endpoint + " -> HTTP " + statusCode + " (" + elapsedMs + " ms)");

void LogFileHandler(string endpoint, int statusCode, long elapsedMs)
    => File.AppendAllText("api_log.txt", DateTime.Now.ToString("u") + " | " + endpoint + " | " + statusCode + " | " + elapsedMs + "ms\n");

delegate void OnRequestCompleted(string endpoint, int statusCode, long elapsedMs);

class ApiService
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost/api/") };
    public event OnRequestCompleted? RequestCompleted;
    public Func<int, Task<string?>> GetById => async (id) => {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync("packages/" + id);
        sw.Stop();
        RequestCompleted?.Invoke("GET /packages/" + id, (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsStringAsync() : null;
    };
    public Action<string> Log => Console.WriteLine;
    public async Task<string> GetAllPackages() {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync("packages");
        sw.Stop();
        RequestCompleted?.Invoke("GET /packages", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return await resp.Content.ReadAsStringAsync();
    }
    public async Task<int> CreatePackage(object package) {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PostAsJsonAsync("packages", package);
        sw.Stop();
        RequestCompleted?.Invoke("POST /packages", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return (int)resp.StatusCode;
    }
    public async Task<int> UpdatePackage(int id, object package) {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PutAsJsonAsync("packages/" + id, package);
        sw.Stop();
        RequestCompleted?.Invoke("PUT /packages/" + id, (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return (int)resp.StatusCode;
    }
    public async Task<int> DeletePackage(int id) {
        var sw = Stopwatch.StartNew();
        var resp = await _http.DeleteAsync("packages/" + id);
        sw.Stop();
        RequestCompleted?.Invoke("DELETE /packages/" + id, (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return (int)resp.StatusCode;
    }
}
