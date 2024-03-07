namespace ncea.enricher.Processor.Contracts;

public interface IOrchestrationService
{
    Task StartProcessorAsync(CancellationToken cancellationToken = default);
}
