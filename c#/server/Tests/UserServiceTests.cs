using Bll.@new;
using Bll.@new.Exceptions;
using Dal;
using Models;
using Moq;
using NUnit.Framework;

namespace Tests;

public class UserServiceTests
{
    [Test]
    public async Task GetUserWhenUserExists()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.GetUserByUsernameAsync("mahdi"))
            .ReturnsAsync(CreateSampleUser("mahdi"));
        var service = new UserService(repository.Object);

        var user = await service.GetUserByUsernameAsync("mahdi");

        Assert.That(user.Username, Is.EqualTo("mahdi"));
    }

    [Test]
    public void GetUserWhenUserMissing()
    {
        var repository = new Mock<IUserRepository>();
        var service = new UserService(repository.Object);

        Assert.ThrowsAsync<UserNotFoundException>(() => service.GetUserByUsernameAsync("nobody"));
    }

    [Test]
    public void RegisterUserWhenUsernameExists()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.GetUserByUsernameAsync("mahdi"))
            .ReturnsAsync(CreateSampleUser("mahdi"));
        var service = new UserService(repository.Object);

        Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            service.RegisterUserAsync("mahdi", "hash", "m@example.com", "Mahdi", "Abbasi"));
        repository.Verify(r => r.InsertUserAsync(It.IsAny<User>()), Times.Never);
    }

    private static User CreateSampleUser(string username)
    {
        return new User
        {
            Id = 1,
            Username = username,
            FirstName = "Mahdi",
            LastName = "Abbasi",
            Email = "m@example.com",
            HashedPassword = "hash"
        };
    }
}
