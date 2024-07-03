using Ncea.Enricher.Models;

namespace Ncea.Enricher.Services.Contracts;

public interface IMdcFieldConfigurationService
{
    List<Field> GetAll();
    List<Field> GetFieldsForClassification();
}
