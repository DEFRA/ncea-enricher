using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Utils;
using Ncea.Harvester.Services.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.Services;

[ExcludeFromCodeCoverage]
public class BackUpService : IBackUpService
{
    private readonly ILogger _logger;

    public BackUpService(ILogger<BackUpService> logger)
    {
        _logger = logger;
    }

    public void CreateNewDataSourceContainerIfNotExist(string dirPath)
    {
        var dir = new DirectoryInfo(dirPath);
        if (!dir.Exists)
            dir.Create();
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

            if (targetDirectory.Exists)
            {
                targetDirectory.Delete(true);
            }
            sourceDirectory.MoveTo(targetDirectory.FullName);
        }
        catch(DirectoryNotFoundException ex)
        {
            CustomLogger.LogErrorMessage(_logger, ex.Message, ex);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occurred while moving file: {sourceDirectory} to {targetDirectory}";
            CustomLogger.LogErrorMessage(_logger, errorMessage, ex);
        }
    }
}