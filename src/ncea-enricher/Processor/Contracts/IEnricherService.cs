namespace Ncea.Enricher.Processors.Contracts;

public interface IEnricherService
{
    Task<string> Enrich(string mappedData, CancellationToken cancellationToken = default);
}