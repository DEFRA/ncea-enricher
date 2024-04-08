using System.Data;

namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IBlobStorageService
{
    Task<DataTable> ReadCsvFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
}
