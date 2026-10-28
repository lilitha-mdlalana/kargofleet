using Api.Controllers;
using Api.Data.Entities;
using Xunit;

namespace Api.UnitTests;

public class DriverControllerTests
{
    private static Trip MakeTrip(string status, DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        DriverId = Guid.NewGuid(),
        VehicleId = Guid.NewGuid(),
        Origin = "A",
        Destination = "B",
        Status = status,
        CreatedAt = createdAt,
    };

    [Fact]
    public void SelectCurrentTrip_PrefersActiveOverScheduled()
    {
        var scheduled = MakeTrip("scheduled", DateTime.UtcNow.AddMinutes(-10));
        var active = MakeTrip("active", DateTime.UtcNow);

        var result = DriverController.SelectCurrentTrip(new[] { scheduled, active });

        Assert.Equal(active.Id, result!.Id);
    }

    [Fact]
    public void SelectCurrentTrip_FallsBackToEarliestScheduled()
    {
        var later = MakeTrip("scheduled", DateTime.UtcNow);
        var earlier = MakeTrip("scheduled", DateTime.UtcNow.AddMinutes(-10));
        var completed = MakeTrip("completed", DateTime.UtcNow.AddMinutes(-20));

        var result = DriverController.SelectCurrentTrip(new[] { later, earlier, completed });

        Assert.Equal(earlier.Id, result!.Id);
    }

    [Fact]
    public void SelectCurrentTrip_ReturnsNullWhenNoneMatch()
    {
        var completed = MakeTrip("completed", DateTime.UtcNow);

        var result = DriverController.SelectCurrentTrip(new[] { completed });

        Assert.Null(result);
    }

    [Fact]
    public void SelectCurrentTrip_ReturnsNullWhenEmpty()
    {
        var result = DriverController.SelectCurrentTrip(Array.Empty<Trip>());

        Assert.Null(result);
    }
}
