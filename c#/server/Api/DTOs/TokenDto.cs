using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class TokenDto
{
    [Required] public required string Token { get; set; }
}