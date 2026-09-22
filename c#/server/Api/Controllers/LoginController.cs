using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    [HttpPost(Name = "LoginUser")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username or email and password are required");
        }

        var response = new LoginResponse
        {
            UsernameOrEmail = request.UsernameOrEmail,
            Message = "Login request received"
        };

        return Ok(response);
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
