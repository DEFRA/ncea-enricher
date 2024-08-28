using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Harvester.Services.Contracts;

public interface IBackUpService
{
    void CreateNewDataSourceContainerIfNotExist(string dirPath);
    void MoveFiles(string sourceDirectoryPath, string targetDirectoryPath);
}
