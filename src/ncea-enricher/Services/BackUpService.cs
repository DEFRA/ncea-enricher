using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Utils;
using Ncea.Harvester.Services.Contracts;

namespace Ncea.Enricher.Services;

public class BackUpService : IBackUpService
{
    private readonly ILogger _logger;

    public BackUpService(ILogger<BackUpService> logger)
    {
        _logger = logger;
    }

    public void MoveFiles(string sourceDirectoryPath, string targetDirectoryPath)
    {
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
        var targetDirectory = new DirectoryInfo(targetDirectoryPath);
        try
        {
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Given datasouce directory not found {sourceDirectory.Name}");
            }

            if (!targetDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Given datasouce directory not found {targetDirectory.Name}");
            }

            DeleteFiles(targetDirectory);
            MoveFiles(sourceDirectory, targetDirectory);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occurred while moving file: {sourceDirectory} to {targetDirectory}";
            CustomLogger.LogErrorMessage(_logger, errorMessage, ex);
        }       
    }

    private static void MoveFiles(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
    {
        sourceDirectory.MoveTo(targetDirectory.FullName);
    }

    private static void DeleteFiles(DirectoryInfo targetDirectory)
    {
        foreach (var file in targetDirectory.GetFiles())
        {
            file.Delete();
        }
    }
}