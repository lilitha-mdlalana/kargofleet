using Api.Data;
using Api.Data.Entities;
using Api.DTOs.Auth;
using Api.DTOs.Driver;
using Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("driver")]
[Authorize]
public class DriverController : ControllerBase
{
    private readonly AppDbContext _db;

    public DriverController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.GetUserId();

        var driver = await _db.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicle)
            .Include(d => d.Trips)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver is null)
        {
            return NotFound(new ErrorResponse("Driver profile not found"));
        }

        var trip = SelectCurrentTrip(driver.Trips);

        var response = new DashboardResponse(
            new DashboardDriver(driver.Id, driver.User.Name, driver.User.Phone, driver.License),
            driver.Vehicle is null
                ? null
                : new DashboardVehicle(driver.Vehicle.Id, driver.Vehicle.Name, driver.Vehicle.Registration),
            trip is null
                ? null
                : new DashboardTrip(trip.Id, trip.Origin, trip.Destination, trip.Status, trip.StartedAt, trip.EndedAt));

        return Ok(response);
    }

    public static Trip? SelectCurrentTrip(IEnumerable<Trip> trips)
    {
        var ordered = trips.OrderBy(t => t.CreatedAt).ToList();
        return ordered.FirstOrDefault(t => t.Status == "active")
            ?? ordered.FirstOrDefault(t => t.Status == "scheduled");
    }
}
