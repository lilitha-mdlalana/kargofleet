using Api.Data;
using Api.Data.Entities;
using Api.Exceptions;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.UnitTests;

public class TripServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(Guid UserId, Guid TripId)> SeedDriverWithTripAsync(AppDbContext db, string status)
    {
        var vehicle = new Vehicle { Id = Guid.NewGuid(), Name = "Test Van", Registration = $"TV-{Guid.NewGuid():N}"[..10] };
        var user = new User { Id = Guid.NewGuid(), Name = "Driver", Phone = $"+27{Random.Shared.Next(100000000, 999999999)}" };
        var driver = new Driver { Id = Guid.NewGuid(), UserId = user.Id, User = user, VehicleId = vehicle.Id, Vehicle = vehicle };
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DriverId = driver.Id,
            Driver = driver,
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            Origin = "A",
            Destination = "B",
            Status = status,
        };

        db.Vehicles.Add(vehicle);
        db.Users.Add(user);
        db.Drivers.Add(driver);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        return (user.Id, trip.Id);
    }

    [Fact]
    public async Task StartTripAsync_ScheduledTrip_SetsActiveAndStartedAt()
    {
        using var db = CreateDb();
        var (userId, tripId) = await SeedDriverWithTripAsync(db, "scheduled");
        var service = new TripService(db);

        var trip = await service.StartTripAsync(userId, tripId);

        Assert.Equal("active", trip.Status);
        Assert.NotNull(trip.StartedAt);
    }

    [Fact]
    public async Task StartTripAsync_AlreadyActiveTrip_Throws400()
    {
        using var db = CreateDb();
        var (userId, tripId) = await SeedDriverWithTripAsync(db, "active");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.StartTripAsync(userId, tripId));

        Assert.Equal(400, ex.Status);
    }

    [Fact]
    public async Task StartTripAsync_CompletedTrip_Throws400()
    {
        using var db = CreateDb();
        var (userId, tripId) = await SeedDriverWithTripAsync(db, "completed");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.StartTripAsync(userId, tripId));

        Assert.Equal(400, ex.Status);
    }

    [Fact]
    public async Task StartTripAsync_UnknownTripId_Throws404()
    {
        using var db = CreateDb();
        var (userId, _) = await SeedDriverWithTripAsync(db, "scheduled");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.StartTripAsync(userId, Guid.NewGuid()));

        Assert.Equal(404, ex.Status);
    }

    [Fact]
    public async Task StartTripAsync_AnotherDriversTrip_Throws404()
    {
        using var db = CreateDb();
        var (_, tripId) = await SeedDriverWithTripAsync(db, "scheduled");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.StartTripAsync(Guid.NewGuid(), tripId));

        Assert.Equal(404, ex.Status);
    }

    [Fact]
    public async Task EndTripAsync_ActiveTrip_SetsCompletedAndEndedAt()
    {
        using var db = CreateDb();
        var (userId, tripId) = await SeedDriverWithTripAsync(db, "active");
        var service = new TripService(db);

        var trip = await service.EndTripAsync(userId, tripId);

        Assert.Equal("completed", trip.Status);
        Assert.NotNull(trip.EndedAt);
    }

    [Fact]
    public async Task EndTripAsync_ScheduledTrip_Throws400()
    {
        using var db = CreateDb();
        var (userId, tripId) = await SeedDriverWithTripAsync(db, "scheduled");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.EndTripAsync(userId, tripId));

        Assert.Equal(400, ex.Status);
    }

    [Fact]
    public async Task EndTripAsync_CompletedTrip_Throws400()
    {
        using var db = CreateDb();
        var (userId, tripId) = await SeedDriverWithTripAsync(db, "completed");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.EndTripAsync(userId, tripId));

        Assert.Equal(400, ex.Status);
    }

    [Fact]
    public async Task EndTripAsync_AnotherDriversTrip_Throws404()
    {
        using var db = CreateDb();
        var (_, tripId) = await SeedDriverWithTripAsync(db, "active");
        var service = new TripService(db);

        var ex = await Assert.ThrowsAsync<TripException>(() => service.EndTripAsync(Guid.NewGuid(), tripId));

        Assert.Equal(404, ex.Status);
    }
}
