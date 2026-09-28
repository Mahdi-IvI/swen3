using System.ComponentModel.DataAnnotations;

namespace Api.Services;

public sealed class JwtOptions
{
    public const string Jwt = "Jwt";

    [Required] public required string Issuer { get; init; }

    [Required] public required string Audience { get; init; }

    public int ExpirationInMinutes { get; init; } = 60;

    [Required]
    [MinLength(32)]
    public required string SecretKey { get; init; }
}
