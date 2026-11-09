using Api.Data;
using Api.DTOs.Auth;
using Api.DTOs.Driver;
using Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("vehicle")]
[Authorize]
public class VehicleController : ControllerBase
{
    private readonly AppDbContext _db;

    public VehicleController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId();
        var driver = await _db.Drivers.Include(d => d.Vehicle).FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver is null)
        {
            return NotFound(new ErrorResponse("Driver profile not found"));
        }

        if (driver.Vehicle is null)
        {
            return NotFound(new ErrorResponse("No vehicle assigned"));
        }

        return Ok(new DashboardVehicle(driver.Vehicle.Id, driver.Vehicle.Name, driver.Vehicle.Registration));
    }
}
