namespace Api.Services;

public class OtpGenerator : IOtpGenerator
{
    public string Generate() => Random.Shared.Next(100000, 1000000).ToString();
}
