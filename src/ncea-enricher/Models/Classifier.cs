namespace Ncea.Enricher.Models;

public class Classifier
{
    public string Id { get; set; } = null!;
    public string? ParentId { get; set; } = null;
    public int Level { get; set; }
    public string Name { get; set; } = null!;
    public List<string>? Synonyms { get; set;}
    public List<Classifier>? Children { get; set; }
}
