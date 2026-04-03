using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;
using DeliveryDesktop.Models;

namespace DeliveryDesktop.Services;

// Собственный делегат для событий API
public delegate void OnRequestCompleted(string endpoint, int statusCode, long elapsedMs);

public class ApiService
{
    private readonly HttpClient _http = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    })
    {
        BaseAddress = new Uri("http://localhost:8080/api/"),
        DefaultRequestVersion = new Version(1, 1)
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true
    };

    // Многоадресный делегат — событие
    public event OnRequestCompleted? RequestCompleted;

    // Action<string> для логирования
    public Action<string> Log => msg =>
        File.AppendAllText("delivery_log.txt",
            $"{DateTime.Now:u} | {msg}\n");

    // Func<int, Task<Package?>> — получение посылки по ID
    public Func<int, Task<Package?>> GetPackageById => async (id) =>
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync($"packages/{id}");
        sw.Stop();
        RequestCompleted?.Invoke($"GET /packages/{id}",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        if (!resp.IsSuccessStatusCode) return null;
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Package>(json, _jsonOptions);
    };

    // Packages
    public async Task<List<Package>> GetAllPackages()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync("packages");
        sw.Stop();
        RequestCompleted?.Invoke("GET /packages",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        if (!resp.IsSuccessStatusCode) return new();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Package>>(json, _jsonOptions) ?? new();
    }

    public async Task<bool> CreatePackage(Package package)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PostAsJsonAsync("packages", package);
        sw.Stop();
        RequestCompleted?.Invoke("POST /packages",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePackage(int id, Package package)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PutAsJsonAsync($"packages/{id}", package);
        sw.Stop();
        RequestCompleted?.Invoke($"PUT /packages/{id}",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePackage(int id)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.DeleteAsync($"packages/{id}");
        sw.Stop();
        RequestCompleted?.Invoke($"DELETE /packages/{id}",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    // Clients
    public async Task<List<Client>> GetAllClients()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync("clients");
        sw.Stop();
        RequestCompleted?.Invoke("GET /clients",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        if (!resp.IsSuccessStatusCode) return new();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Client>>(json, _jsonOptions) ?? new();
    }

    // Couriers
    public async Task<List<Courier>> GetAllCouriers()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync("couriers");
        sw.Stop();
        RequestCompleted?.Invoke("GET /couriers",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        if (!resp.IsSuccessStatusCode) return new();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Courier>>(json, _jsonOptions) ?? new();
    }

    // Routes
    public async Task<List<Route>> GetAllRoutes()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.GetAsync("routes");
        sw.Stop();
        RequestCompleted?.Invoke("GET /routes",
            (int)resp.StatusCode, sw.ElapsedMilliseconds);
        if (!resp.IsSuccessStatusCode) return new();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Route>>(json, _jsonOptions) ?? new();
    }

    // Clients
    public async Task<bool> CreateClient(Client client)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PostAsJsonAsync("clients", client);
        sw.Stop();
        RequestCompleted?.Invoke("POST /clients", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteClient(int id)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.DeleteAsync($"clients/{id}");
        sw.Stop();
        RequestCompleted?.Invoke($"DELETE /clients/{id}", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    // Couriers
    public async Task<bool> CreateCourier(Courier courier)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PostAsJsonAsync("couriers", courier);
        sw.Stop();
        RequestCompleted?.Invoke("POST /couriers", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCourier(int id)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.DeleteAsync($"couriers/{id}");
        sw.Stop();
        RequestCompleted?.Invoke($"DELETE /couriers/{id}", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    // Routes
    public async Task<bool> CreateRoute(Route route)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.PostAsJsonAsync("routes", route);
        sw.Stop();
        RequestCompleted?.Invoke("POST /routes", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRoute(int id)
    {
        var sw = Stopwatch.StartNew();
        var resp = await _http.DeleteAsync($"routes/{id}");
        sw.Stop();
        RequestCompleted?.Invoke($"DELETE /routes/{id}", (int)resp.StatusCode, sw.ElapsedMilliseconds);
        return resp.IsSuccessStatusCode;
    }
}