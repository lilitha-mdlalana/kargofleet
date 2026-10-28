namespace Api.DTOs.Auth;

public record MessageResponse(string Message);

public record ErrorResponse(string Message);

public record UserDto(Guid Id, string Name, string Phone);

public record VerifyResponse(string Token, UserDto User);
