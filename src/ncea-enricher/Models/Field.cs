using Ncea.Enricher.Enums;

namespace Ncea.Enricher.Models;

public class Field
{
    public FieldName Name { get; set; }
    public FieldType Type { get; set; }
    public string XPath { get; set; } = null!;
    public string ConditionalChild { get; set; } = null!;
    public string RelatedChild { get; set; } = null!;
    public List<ResourceType> RelevantFor { get; set; } = null!;
    public MdcObligation Obligation { get; set; }
    public bool UsedForNceaProfiling { get; set; }
}
