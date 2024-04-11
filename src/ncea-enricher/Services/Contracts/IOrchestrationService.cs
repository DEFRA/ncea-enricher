namespace Ncea.Enricher.Services.Contracts;

public interface IOrchestrationService
{
    Task StartProcessorAsync(CancellationToken cancellationToken = default);
}
