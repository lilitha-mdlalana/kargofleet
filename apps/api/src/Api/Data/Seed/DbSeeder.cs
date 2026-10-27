using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Seed;

public static class DbSeeder
{
    private const string DemoPhone = "+27821234567";

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Phone == DemoPhone))
        {
            return;
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Toyota Quantum",
            Registration = "CA 123-456",
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Phone = DemoPhone,
            Name = "Thando Mokoena",
            Driver = new Driver
            {
                Id = Guid.NewGuid(),
                License = "C1",
                Vehicle = vehicle,
            },
        };

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Driver = user.Driver,
            Vehicle = vehicle,
            Origin = "Cape Town",
            Destination = "Stellenbosch",
            Status = "scheduled",
        };

        db.Vehicles.Add(vehicle);
        db.Users.Add(user);
        db.Trips.Add(trip);

        await db.SaveChangesAsync();
    }
}
