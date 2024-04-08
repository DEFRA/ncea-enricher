using Ncea.Enricher.Models;

namespace Ncea.Enricher.Processors.Contracts;

public interface ISynonymsProvider
{
    Task<Classifiers> GetAll(CancellationToken cancellationToken);
}
