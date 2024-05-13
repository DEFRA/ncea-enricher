using Ncea.Enricher.Models;

namespace Ncea.Enricher.Services.Contracts;

public interface ISearchableFieldConfigurations
{
    List<Field> GetAll();
}
