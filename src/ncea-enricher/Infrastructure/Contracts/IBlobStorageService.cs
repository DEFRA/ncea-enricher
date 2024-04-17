using System.Data;

namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IBlobStorageService
{
    Task<DataTable> ReadExcelFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
}
