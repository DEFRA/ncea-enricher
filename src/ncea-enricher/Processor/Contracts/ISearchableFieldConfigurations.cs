using Ncea.Enricher.Models;

namespace Ncea.Enricher.Processor.Contracts;

public interface ISearchableFieldConfigurations
{
    List<SearchableField> GetSearchableFieldConfigurations();
}
