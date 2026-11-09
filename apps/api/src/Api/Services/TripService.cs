using Api.Data;
using Api.Data.Entities;
using Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TripService : ITripService
{
    private readonly AppDbContext _db;

    public TripService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Trip> StartTripAsync(Guid userId, Guid tripId)
    {
        var trip = await LoadOwnedTripAsync(userId, tripId);

        if (trip.Status != "scheduled")
        {
            throw new TripException(400, "Trip is not scheduled");
        }

        trip.Status = "active";
        trip.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return trip;
    }

    public async Task<Trip> EndTripAsync(Guid userId, Guid tripId)
    {
        var trip = await LoadOwnedTripAsync(userId, tripId);

        if (trip.Status != "active")
        {
            throw new TripException(400, "Trip is not active");
        }

        trip.Status = "completed";
        trip.EndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return trip;
    }

    private async Task<Trip> LoadOwnedTripAsync(Guid userId, Guid tripId)
    {
        var trip = await _db.Trips.Include(t => t.Driver).FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null || trip.Driver.UserId != userId)
        {
            throw new TripException(404, "Trip not found");
        }

        return trip;
    }
}
