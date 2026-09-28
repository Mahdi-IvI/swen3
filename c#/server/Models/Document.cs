using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class Document
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string FileName { get; set; }
    [NotMapped] public string Summery { get; set; } = string.Empty;
}
