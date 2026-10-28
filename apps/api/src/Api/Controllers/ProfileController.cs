using Api.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(501, new ErrorResponse("not implemented"));

    [HttpPut]
    public IActionResult Update() => StatusCode(501, new ErrorResponse("not implemented"));
}
