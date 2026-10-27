namespace Api.Data.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Registration { get; set; } = default!;
    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
