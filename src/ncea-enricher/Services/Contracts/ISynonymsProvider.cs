using Ncea.Enricher.Models;

namespace Ncea.Enricher.Services.Contracts;

public interface ISynonymsProvider
{
    Task<List<Classifier>> GetAll(CancellationToken cancellationToken);
}
