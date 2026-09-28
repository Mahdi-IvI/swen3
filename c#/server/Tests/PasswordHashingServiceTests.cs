using Api.Services;
using Xunit;

namespace Tests;

public class PasswordHashingServiceTests
{
    [Fact]
    public void VerifyCorrectPassword()
    {
        var service = new PasswordHashingService();
        var hash = service.Hash("secret123");

        var result = service.Verify(hash, "secret123");

        Assert.True(result);
    }

    [Fact]
    public void VerifyWrongPassword()
    {
        var service = new PasswordHashingService();
        var hash = service.Hash("secret123");

        var result = service.Verify(hash, "wrong-password");

        Assert.False(result);
    }
}
