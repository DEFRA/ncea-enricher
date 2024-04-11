namespace Ncea.Enricher.Models;

public class Classifier : IEquatable<Classifier>
{
    public string Id { get; set; } = null!;
    public string? ParentId { get; set; } = null;
    public int Level { get; set; }
    public string Name { get; set; } = null!;
    public List<string>? Synonyms { get; set;}
    public List<Classifier>? Children { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as Classifier);
    }

    public bool Equals(Classifier classifier)
    {
        return classifier != null && Id == classifier.Id && ParentId == classifier.ParentId && Name == classifier.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, ParentId, Name);
    }
}
