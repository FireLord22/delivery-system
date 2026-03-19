using Microsoft.EntityFrameworkCore;
using DeliveryApi.Models;

namespace DeliveryApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Models.Route> Routes => Set<Models.Route>();
    public DbSet<Package> Packages => Set<Package>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seeding — тестовые данные
        modelBuilder.Entity<Client>().HasData(
            new Client { Id = 1, FullName = "Иван Петров", Phone = "79001234567", Address = "Москва, ул. Ленина 1", Email = "ivan@mail.ru" },
            new Client { Id = 2, FullName = "Мария Сидорова", Phone = "79007654321", Address = "Москва, ул. Пушкина 5", Email = "maria@mail.ru" }
        );
        modelBuilder.Entity<Courier>().HasData(
            new Courier { Id = 1, FullName = "Алексей Курьеров", Phone = "79009876543", Vehicle = "Велосипед", IsAvailable = true },
            new Courier { Id = 2, FullName = "Дмитрий Быстров", Phone = "79001112233", Vehicle = "Мотоцикл", IsAvailable = true }
        );
        modelBuilder.Entity<Models.Route>().HasData(
            new Models.Route { Id = 1, Name = "Маршрут А", StartPoint = "Склад Центральный", EndPoint = "Район Северный", DistanceKm = 12.5 },
            new Models.Route { Id = 2, Name = "Маршрут Б", StartPoint = "Склад Центральный", EndPoint = "Район Южный", DistanceKm = 18.3 }
        );
        modelBuilder.Entity<Package>().HasData(
            new Package { Id = 1, TrackingNumber = "TRK001", WeightKg = 2.5, Status = "InTransit", CreatedAt = DateTime.UtcNow, ClientId = 1, CourierId = 1, RouteId = 1 },
            new Package { Id = 2, TrackingNumber = "TRK002", WeightKg = 0.8, Status = "Pending", CreatedAt = DateTime.UtcNow, ClientId = 2, CourierId = 2, RouteId = 2 }
        );
    }
}