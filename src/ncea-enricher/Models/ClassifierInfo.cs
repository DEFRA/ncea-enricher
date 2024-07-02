namespace Ncea.Enricher.Models;

public sealed class ClassifierInfo : IEquatable<ClassifierInfo>
{
    public string Id { get; set; } = null!;
    public string? ParentId { get; set; } = null;
    public int Level { get; set; }
    public string Name { get; set; } = null!;
    public List<string>? Synonyms { get; set;}
    public List<ClassifierInfo>? Children { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        return Equals(obj as ClassifierInfo);
    }

    public bool Equals(ClassifierInfo? classifier)
    {
        return classifier != null && Id == classifier.Id && ParentId == classifier.ParentId && Name == classifier.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, ParentId, Name);
    }
}
