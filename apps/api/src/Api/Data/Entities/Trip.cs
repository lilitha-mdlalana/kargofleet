namespace Api.Data.Entities;

public class Trip
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = default!;
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;
    public string Origin { get; set; } = default!;
    public string Destination { get; set; } = default!;
    public string Status { get; set; } = "scheduled";
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
