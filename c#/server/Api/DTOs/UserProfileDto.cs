using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class UserProfileDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}

public class UpdateProfileDto
{
    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [StringLength(30, MinimumLength = 8)]
    public string? Password { get; set; }
}
