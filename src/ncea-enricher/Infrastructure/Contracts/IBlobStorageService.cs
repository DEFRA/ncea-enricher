using Ncea.Enricher.Infrastructure.Models.Requests;

namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IBlobStorageService
{
    Task<string> SaveAsync(SaveBlobRequest request, CancellationToken cancellationToken = default);
}
