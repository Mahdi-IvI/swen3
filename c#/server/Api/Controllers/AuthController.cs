using System.Security.Claims;
using Api.DTOs;
using Api.Services;
using Bll.@new;
using Bll.@new.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    ITokenService tokenService,
    IUserService userService,
    IPasswordHashingService passwordHashingService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] CredentialsDto credentials)
    {
        try
        {
            var user = await userService.GetUserByUsernameAsync(credentials.Username);
            if (!passwordHashingService.Verify(user.HashedPassword, credentials.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(CreateTokenDto(user));
        }
        catch (UserNotFoundException)
        {
            return Unauthorized("Invalid credentials");
        }
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<TokenDto>> Register([FromBody] RegisterDto registration)
    {
        var hashedPassword = passwordHashingService.Hash(registration.Password);

        try
        {
            var user = await userService.RegisterUserAsync(
                registration.Username,
                hashedPassword,
                registration.Email,
                registration.FirstName,
                registration.LastName);

            return Created(string.Empty, CreateTokenDto(user));
        }
        catch (UserAlreadyExistsException)
        {
            return Conflict("Username or email already exists");
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            var user = await userService.GetUserByUsernameAsync(username);
            return Ok(ToProfileDto(user));
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateCurrentUser([FromBody] UpdateProfileDto profile)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            var hashedPassword = string.IsNullOrWhiteSpace(profile.Password)
                ? null
                : passwordHashingService.Hash(profile.Password);
            var user = await userService.UpdateProfileAsync(
                username,
                profile.FirstName,
                profile.LastName,
                hashedPassword);

            return Ok(ToProfileDto(user));
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }

    private TokenDto CreateTokenDto(User user)
    {
        return new TokenDto { Token = tokenService.GenerateToken(user) };
    }

    private static UserProfileDto ToProfileDto(User user) => new()
    {
        Username = user.Username,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email
    };

    private string? GetUsername()
    {
        return User.FindFirstValue(ClaimTypes.Name);
    }
}
