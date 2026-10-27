using Api.Data.Entities;

namespace Api.Services;

public interface IAuthService
{
    Task<User> StartSignupAsync(string name, string phone);
    Task<User> StartLoginAsync(string phone);
    Task<User> VerifyOtpAsync(string phone, string code);
}
