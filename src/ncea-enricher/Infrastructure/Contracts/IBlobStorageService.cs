namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IBlobStorageService
{
    Task<string> ReadCsvFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
}
