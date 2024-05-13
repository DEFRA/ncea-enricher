using Ncea.Enricher.Constants;

namespace Ncea.Enricher.Models;

public class Field
{
    public FieldName Name { get; set; }
    public FieldType Type { get; set; }
    public string XPath { get; set; } = null!;
    public string ConditionalChild { get; set; } = null!;
    public string RelatedChild { get; set; } = null!;
    public ResourceType[] RelevantFor { get; set; } = null!;
}
