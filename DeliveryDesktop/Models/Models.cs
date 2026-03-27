namespace DeliveryDesktop.Models;

public class Package
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ClientId { get; set; }
    public int CourierId { get; set; }
    public int RouteId { get; set; }
    public Client? Client { get; set; }
    public Courier? Courier { get; set; }
    public Route? Route { get; set; }
}

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class Courier
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class Route
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}

public class User
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" или "admin"
}