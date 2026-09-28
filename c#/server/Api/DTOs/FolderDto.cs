using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class FolderDto
{
    public int Id { get; set; }

    [Required] public required string Name { get; set; }

    [Required] public required string Description { get; set; }

    public int? ContainingFolder { get; set; }
}
