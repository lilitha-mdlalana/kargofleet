using Api.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => StatusCode(501, new ErrorResponse("not implemented"));

    [HttpPost]
    public IActionResult Create() => StatusCode(501, new ErrorResponse("not implemented"));

    [HttpDelete("{id}")]
    public IActionResult Delete(string id) => StatusCode(501, new ErrorResponse("not implemented"));
}
