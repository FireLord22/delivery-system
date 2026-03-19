namespace DeliveryApi.Models;

public class Route
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public double DistanceKm { get; set; }

    public ICollection<Package> Packages { get; set; } = new List<Package>();
}