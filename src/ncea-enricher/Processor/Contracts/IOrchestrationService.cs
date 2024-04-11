namespace Ncea.Enricher.Processor.Contracts;

public interface IOrchestrationService
{
    Task StartProcessorAsync(CancellationToken cancellationToken = default);
}
