using Api.DTOs.Auth;
using Api.Exceptions;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Phone))
        {
            return BadRequest(new ErrorResponse("name and phone are required"));
        }

        try
        {
            await _authService.StartSignupAsync(req.Name.Trim(), req.Phone.Trim());
            return StatusCode(202, new MessageResponse("OTP sent"));
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.Status, new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Phone))
        {
            return BadRequest(new ErrorResponse("phone is required"));
        }

        try
        {
            await _authService.StartLoginAsync(req.Phone.Trim());
            return StatusCode(202, new MessageResponse("OTP sent"));
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.Status, new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Phone) || string.IsNullOrWhiteSpace(req.Code))
        {
            return BadRequest(new ErrorResponse("phone and code are required"));
        }

        try
        {
            var user = await _authService.VerifyOtpAsync(req.Phone.Trim(), req.Code.Trim());
            var token = _jwtService.Sign(user.Id);
            return Ok(new VerifyResponse(token, new UserDto(user.Id, user.Name, user.Phone)));
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.Status, new ErrorResponse(ex.Message));
        }
    }
}
