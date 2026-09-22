using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
{
    [HttpPost(Name = "RegisterUser")]
    public ActionResult<RegistrationResponse> Register(RegistrationRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Password and password conformation don't match");
        }

        var response = new RegistrationResponse
        {
            Username = request.Username,
            Email = request.Email,
            Message = "Registration request received"
        };

        return Ok(response);
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
