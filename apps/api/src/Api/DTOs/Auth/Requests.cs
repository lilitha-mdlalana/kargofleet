namespace Api.DTOs.Auth;

public record SignupRequest(string? Name, string? Phone);

public record LoginRequest(string? Phone);

public record VerifyRequest(string? Phone, string? Code);
