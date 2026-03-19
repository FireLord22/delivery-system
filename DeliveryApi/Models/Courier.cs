namespace DeliveryApi.Models;

public class Courier
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty; // Тип транспорта
    public bool IsAvailable { get; set; } = true;

    public ICollection<Package> Packages { get; set; } = new List<Package>();
}