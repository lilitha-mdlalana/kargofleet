using Api.Data.Entities;

namespace Api.Services;

public interface ITripService
{
    Task<Trip> StartTripAsync(Guid userId, Guid tripId);
    Task<Trip> EndTripAsync(Guid userId, Guid tripId);
}
