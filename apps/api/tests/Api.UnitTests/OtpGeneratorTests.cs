using Api.Services;
using Xunit;

namespace Api.UnitTests;

public class OtpGeneratorTests
{
    [Fact]
    public void Generate_ReturnsSixDigitNumericCodeInRange()
    {
        var generator = new OtpGenerator();

        var code = generator.Generate();

        Assert.Equal(6, code.Length);
        Assert.True(int.TryParse(code, out var value));
        Assert.InRange(value, 100000, 999999);
    }
}
