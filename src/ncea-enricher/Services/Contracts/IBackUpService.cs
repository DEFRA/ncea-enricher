namespace Ncea.Enricher.Services.Contracts;

public interface IBackUpService
{
    void CreateNewDataSourceContainerIfNotExist(string dirPath);
    void MoveFiles(string sourceDirectoryPath, string targetDirectoryPath);
}
