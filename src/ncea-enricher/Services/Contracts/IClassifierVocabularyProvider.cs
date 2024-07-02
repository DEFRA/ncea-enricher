using Ncea.Enricher.Models;

namespace Ncea.Enricher.Services.Contracts;

public interface ISynonymsProvider
{
    Task<List<ClassifierInfo>> GetAll(CancellationToken cancellationToken);
}
