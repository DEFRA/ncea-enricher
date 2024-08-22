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

    public void CreateDirectory(ICustomDirectoryInfoWrapper directory)
    {
        try
        {
            CreateDirectoryWithPath(directory);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occurred while creating directory: {directory.Name}";
            CustomLogger.LogErrorMessage(_logger, errorMessage, ex);
        }
    }

    private static void CreateDirectoryWithPath(ICustomDirectoryInfoWrapper directory)
    {
        if (!directory.Exists)
        {
            directory.Create();
        }
    }

    public void MoveFiles(ICustomDirectoryInfoWrapper sourceDirectory, ICustomDirectoryInfoWrapper targetDirectory)
    {
        try
        {
            RenameFolder(sourceDirectory, targetDirectory);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occurred while moving file: {sourceDirectory} to {targetDirectory}";
            CustomLogger.LogErrorMessage(_logger, errorMessage, ex);
        }       
    }

    private static void RenameFolder(ICustomDirectoryInfoWrapper sourceDirectory, ICustomDirectoryInfoWrapper targetDirectory)
    {
        if (!sourceDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Given datasouce directory not found {sourceDirectory.Name}");
        }

        if (!targetDirectory.Exists)
        {
            targetDirectory.Create();
        }
        else
        {
            foreach (var file in targetDirectory.GetFiles())
            {
                file.Delete();
            }
        }

        sourceDirectory.MoveTo(targetDirectory.FullName);
    }
}