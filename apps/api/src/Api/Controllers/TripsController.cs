using Api.Data.Entities;
using Api.DTOs.Auth;
using Api.DTOs.Driver;
using Api.Exceptions;
using Api.Extensions;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("trips")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        try
        {
            var trip = await _tripService.StartTripAsync(User.GetUserId(), id);
            return Ok(ToDashboardTrip(trip));
        }
        catch (TripException ex)
        {
            return StatusCode(ex.Status, new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id}/end")]
    public async Task<IActionResult> End(Guid id)
    {
        try
        {
            var trip = await _tripService.EndTripAsync(User.GetUserId(), id);
            return Ok(ToDashboardTrip(trip));
        }
        catch (TripException ex)
        {
            return StatusCode(ex.Status, new ErrorResponse(ex.Message));
        }
    }

    private static DashboardTrip ToDashboardTrip(Trip trip) =>
        new(trip.Id, trip.Origin, trip.Destination, trip.Status, trip.StartedAt, trip.EndedAt);
}
