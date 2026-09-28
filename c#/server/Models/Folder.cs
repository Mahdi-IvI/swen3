namespace Models;

public class Folder
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int? ContainingFolder { get; set; }
}