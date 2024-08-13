namespace Ncea.Harvester.Services.Contracts;

public interface IBackUpService
{
    void MoveFiles(string sourceDirectory, string targetDirectory);
    void CreateDirectory(string directoryName);
}
