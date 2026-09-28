using Api.Services;
using NUnit.Framework;

namespace Tests;

public class PasswordHashingServiceTests
{
    [Test]
    public void VerifyCorrectPassword()
    {
        var service = new PasswordHashingService();
        var hash = service.Hash("secret123");

        var result = service.Verify(hash, "secret123");

        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyWrongPassword()
    {
        var service = new PasswordHashingService();
        var hash = service.Hash("secret123");

        var result = service.Verify(hash, "wrong-password");

        Assert.That(result, Is.False);
    }
}
