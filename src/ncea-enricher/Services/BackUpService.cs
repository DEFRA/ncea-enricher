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

    public void CreateDirectory(IDirectoryInfoWrapper directory)
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

    private static void CreateDirectoryWithPath(IDirectoryInfoWrapper directory)
    {
        if (!directory.Exists)
        {
            directory.Create();
        }
    }

    public void MoveFiles(IDirectoryInfoWrapper sourceDirectory, IDirectoryInfoWrapper targetDirectory)
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

    private static void RenameFolder(IDirectoryInfoWrapper sourceDirectory, IDirectoryInfoWrapper targetDirectory)
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