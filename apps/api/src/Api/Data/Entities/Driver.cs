namespace Api.Data.Entities;

public class Driver
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string? License { get; set; }
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
