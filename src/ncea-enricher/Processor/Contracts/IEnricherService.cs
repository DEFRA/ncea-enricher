namespace Ncea.Enricher.Processor.Contracts;

public interface IEnricherService
{
    Task<string> Enrich(string mappedData, CancellationToken cancellationToken = default);
}