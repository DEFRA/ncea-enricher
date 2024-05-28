using Ncea.Enricher.Infrastructure.Models.Requests;
using System.Data;

namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IBlobService
{
    Task<string> GetContentAsync(GetBlobContentRequest request, CancellationToken cancellationToken);
    Task DeleteBlobAsync(DeleteBlobRequest request, CancellationToken cancellationToken);
    Task<DataTable> ReadExcelFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);    
}
