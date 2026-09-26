using Api.Services;
using Dal;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController(IUserRepository users, IPasswordHashingService passwordHashing) : ControllerBase
{
    [HttpPost(Name = "LoginUser")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username or email and password are required");

        var user = await users.FindByUsernameOrEmailAsync(request.UsernameOrEmail.Trim());
        if (user is null || !passwordHashing.Verify(user.HashedPassword, request.Password))
            return Unauthorized("Invalid credentials");

        return Ok(new LoginResponse
        {
            UsernameOrEmail = user.Username,
            Message = "Login successful"
        });
    }
}

public class LoginRequest
{
    public required string UsernameOrEmail { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse
{
    public required string UsernameOrEmail { get; set; }
    public required string Message { get; set; }
}
