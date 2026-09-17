namespace Models;

public class Document
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string FileName { get; set; }
    public required string Summery { get; set; }
}