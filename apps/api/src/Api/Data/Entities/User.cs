namespace Api.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Driver? Driver { get; set; }
}
