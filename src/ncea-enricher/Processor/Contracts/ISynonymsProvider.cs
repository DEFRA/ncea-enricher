using Ncea.Enricher.Models;

namespace Ncea.Enricher.Processor.Contracts;

public interface ISynonymsProvider
{
    Task<List<Classifier>> GetAll(CancellationToken cancellationToken);
}
