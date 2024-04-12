namespace Ncea.Enricher.Processor.Contracts;

public interface IEnricherService
{
    Task<string> Enrich(string fileIdentifier, string mappedData, CancellationToken cancellationToken = default);
}