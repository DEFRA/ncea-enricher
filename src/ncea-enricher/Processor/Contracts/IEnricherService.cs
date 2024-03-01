namespace Ncea.Enricher.Processors.Contracts;

public interface IEnricherService
{
    Task<string> Transform(string harvestedData, CancellationToken cancellationToken = default);
}