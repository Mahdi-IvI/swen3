using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class DocumentDto
{
    public int Id { get; set; }

    [Required] public required string Name { get; set; }

    [Required] public required string Description { get; set; }

    [Required] public required string FileName { get; set; }
}