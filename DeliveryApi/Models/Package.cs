namespace DeliveryApi.Models;

public class Package
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public int CourierId { get; set; }
    public Courier? Courier { get; set; }

    public int RouteId { get; set; }
    public Route? Route { get; set; }
}