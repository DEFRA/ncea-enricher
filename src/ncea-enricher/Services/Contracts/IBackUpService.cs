using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Harvester.Services.Contracts;

public interface IBackUpService
{
    void MoveFiles(ICustomDirectoryInfoWrapper sourceDirectory, ICustomDirectoryInfoWrapper targetDirectory);
    void CreateDirectory(ICustomDirectoryInfoWrapper directory);
}
