using Ncea.Enricher.Enums;

namespace Ncea.Enricher.Models;

public class MdcMappedRecordMessage
{
    public string FileIdentifier { get; set; } = null!;
    public DataSource DataSource { get; set; }
}

