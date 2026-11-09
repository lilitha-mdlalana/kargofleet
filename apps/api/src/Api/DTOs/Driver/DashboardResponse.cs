namespace Api.DTOs.Driver;

public record DashboardDriver(Guid Id, string Name, string Phone, string? License);

public record DashboardVehicle(Guid Id, string Name, string Registration);

public record DashboardTrip(Guid Id, string Origin, string Destination, string Status, DateTime? StartedAt, DateTime? EndedAt);

public record DashboardResponse(DashboardDriver Driver, DashboardVehicle? Vehicle, DashboardTrip? Trip);
