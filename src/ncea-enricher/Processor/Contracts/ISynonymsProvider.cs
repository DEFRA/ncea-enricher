using Ncea.Enricher.Models;

namespace Ncea.Enricher.Processors.Contracts;

public interface ISynonymsProvider
{
    Task<IList<Classifier>> GetAll(CancellationToken cancellationToken);
}
