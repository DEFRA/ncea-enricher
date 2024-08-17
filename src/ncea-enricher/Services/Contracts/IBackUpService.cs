using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Harvester.Services.Contracts;

public interface IBackUpService
{
    void MoveFiles(IDirectoryInfoWrapper sourceDirectory, IDirectoryInfoWrapper targetDirectory);
    void CreateDirectory(IDirectoryInfoWrapper directory);
}
