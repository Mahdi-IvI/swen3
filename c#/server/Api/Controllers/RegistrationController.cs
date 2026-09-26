using Api.Services;
using Dal;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController(IUserRepository users, IPasswordHashingService passwordHashing) : ControllerBase
{
    [HttpPost(Name = "RegisterUser")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username, email and password are required");

        if (request.Password != request.ConfirmPassword)
            return BadRequest("Password and password confirmation don't match");

        if (await users.FindByUsernameOrEmailAsync(request.Username, request.Email) is not null)
            return Conflict("Username or email already exists");

        var user = new User
        {
            Id = 0,
            Username = request.Username.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            HashedPassword = passwordHashing.Hash(request.Password)
        };

        try
        {
            await users.AddAsync(user);
        }
        catch (UserAlreadyExistsException)
        {
            return Conflict("Username or email already exists");
        }

        return Created(string.Empty, new RegistrationResponse
        {
            Username = user.Username,
            Email = user.Email,
            Message = "Registration successful"
        });
    }
}

public class RegistrationRequest
{
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class RegistrationResponse
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Message { get; set; }
}
