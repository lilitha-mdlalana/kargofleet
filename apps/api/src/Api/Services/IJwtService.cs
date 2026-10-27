namespace Api.Services;

public interface IJwtService
{
    string Sign(Guid userId);
}
