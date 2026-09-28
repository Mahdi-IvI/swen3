using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class CredentialsDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
