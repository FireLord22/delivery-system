namespace DeliveryApi.Models;

public class Package
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/InTransit/Delivered
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int CourierId { get; set; }
    public Courier Courier { get; set; } = null!;

    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;
}