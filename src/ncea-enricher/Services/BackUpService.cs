using Azure;
using Ncea.Harvester.Services.Contracts;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Utils;
using Ncea.Enricher.Enums;
using System.IO;

namespace Ncea.Enricher.Services;

public class BackUpService : IBackUpService
{
    private readonly string _fileSharePath;
    private readonly ILogger _logger;

    public BackUpService(IConfiguration configuration, ILogger<BackUpService> logger)
    {
        _fileSharePath = configuration.GetValue<string>("FileShareName")!;
        _logger = logger;
    }

    public void CreateDirectory(string directoryName)
    {
        try
        {
            var dirPath = Path.Combine(_fileSharePath!, directoryName.ToLowerInvariant());
            CreateDirectoryWithPath(dirPath);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occurred while creating directory: {directoryName}";
            CustomLogger.LogErrorMessage(_logger, errorMessage, ex);
        }
    }

    private static void CreateDirectoryWithPath(string dirPath)
    {
        var dirInfo = new DirectoryInfo(dirPath);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }
    }

    public void MoveFiles(string sourceDirectory, string targetDirectory)
    {
        try
        {
            var srcDirPath = Path.Combine(_fileSharePath, sourceDirectory);
            var targetDirPath = Path.Combine(_fileSharePath, targetDirectory);

            RenameFolder(srcDirPath, targetDirPath);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occurred while moving file: {sourceDirectory} to {targetDirectory}";
            CustomLogger.LogErrorMessage(_logger, errorMessage, ex);
        }       
    }

    private static void RenameFolder(string sourceDirectoryPath, string targetDirectoryPath)
    {
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
        var targetDirectory = new DirectoryInfo(targetDirectoryPath);

        if (!sourceDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Given datasouce directory not found {sourceDirectoryPath}");
        }

        if (!targetDirectory.Exists)
        {
            targetDirectory.Create();
        }
        else
        {
            foreach (FileInfo file in targetDirectory.GetFiles())
            {
                file.Delete();
            }
        }

        sourceDirectory.MoveTo(targetDirectoryPath);
    }
}
