using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Harvester.Services;
using FluentAssertions;
using Azure;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Services;
using Microsoft.Extensions.FileProviders;
using System.IO.Abstractions;

namespace Ncea.Harvester.Tests.Processors;

public class BackUpServiceTests
{
    private readonly BackUpService _backupService;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<BackUpService>> _loggerMock;

    public BackUpServiceTests()
    {
        _loggerMock = new Mock<ILogger<BackUpService>>();
        _loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );

        List<KeyValuePair<string, string?>> lstProps =
            [
                new KeyValuePair<string, string?>("FileShareName", Directory.GetCurrentDirectory()),
            ];

        _configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();
        _backupService = new BackUpService(_configuration, _loggerMock.Object);
        
    }

    [Fact]
    public void MoveFilesn_WhenSourceDirectory_NotExist_ThrowAnException()
    {
        //Arrange
        //var dataSourceName = string.Empty;
        //var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
        //var _srcDirectoryInfoMock = GetDirectoryInfo(@"\\test-fileshare\SourceDirectory", false, _fileInfoMock);
        //var _trgDirectoryInfoMock = GetDirectoryInfo(@"\\test-fileshare\targetDirectory", true, _fileInfoMock);


        //Act
        _backupService.MoveFiles(@"\\test-fileshare\SourceDirectory", @"\\test-fileshare\targetDirectory");

        //Assert
        _loggerMock.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(1),
            It.IsAny<string>()
        );
    }

    //[Fact]
    //public void MoveFilesn_When_SourceDirectory_Exist_TargetDirectory_NotExist()
    //{
    //    //Arrange
    //    var dataSourceName = string.Empty;
    //    var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
    //    var _srcDirectoryInfoMock = GetDirectoryInfo(@"\\test-fileshare\SourceDirectory", true, _fileInfoMock);
    //    var _trgDirectoryInfoMock = GetDirectoryInfo(@"\\test-fileshare\targetDirectory", false, _fileInfoMock);


    //    //Act
    //    _backupService.MoveFiles(@"\\test-fileshare\SourceDirectory", @"\\test-fileshare\targetDirectory");

    //    //Assert
    //    _trgDirectoryInfoMock.Verify(x => x.Create(), Times.Once);
    //    _srcDirectoryInfoMock.Verify(x => x.MoveTo(It.IsNotNull<string>()), Times.Once);
    //}

    //[Fact]
    //public void MoveFilesn_When_SourceDirectory_Exist_TargetDirectory_Exist()
    //{
    //    //Arrange
    //    var dataSourceName = string.Empty;
    //    var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
    //    var _srcDirectoryInfoMock = GetDirectoryInfo(@"\\test-fileshare\SourceDirectory", true, _fileInfoMock);
    //    var _trgDirectoryInfoMock = GetDirectoryInfo(@"\\test-fileshare\targetDirectory", true, _fileInfoMock);


    //    //Act
    //    _backupService.MoveFiles(@"\\test-fileshare\SourceDirectory", @"\\test-fileshare\targetDirectory");

    //    //Assert
    //    _trgDirectoryInfoMock.Verify(x => x.GetFiles(), Times.Once);
    //    _fileInfoMock.Verify(x => x.Delete(), Times.Once);
    //    _srcDirectoryInfoMock.Verify(x => x.MoveTo(It.IsNotNull<string>()), Times.Once);
    //}

    //[Fact]
    //public void CreateDirectory_When_Directory_Exist()
    //{
    //    //Arrange
    //    var dataSourceName = string.Empty;
    //    var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
    //    var _directoryInfoMock = GetDirectoryInfo(It.IsNotNull<string>(), true, _fileInfoMock);


    //    //Act
    //    _backupService.CreateDirectory(@"targetDirectory");

    //    //Assert
    //    _directoryInfoMock.Verify(x => x.Create(), Times.Never);
    //}

    //[Fact]
    //public void CreateDirectory_When_Directory_NotExist()
    //{
    //    //Arrange
    //    var dataSourceName = string.Empty;
    //    var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
    //    var _directoryInfoMock = GetDirectoryInfo(It.IsNotNull<string>(), false, _fileInfoMock);

    //    //Act
    //    _backupService.CreateDirectory(@"targetDirectory");

    //    //Assert
    //    _directoryInfoMock.Verify(x => x.Create(), Times.Once);
    //}

    [Fact]
    public void CreateDirectory_When_DirectoryName_IsNull()
    {
        //Arrange
        //Act
        _backupService.CreateDirectory(null!);

        //Assert
        _loggerMock.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(1),
            It.IsAny<string>()
        );
    }

    private Mock<IDirectoryInfo> GetDirectoryInfo(string directoryPath, bool isDirectoryExisting, Mock<System.IO.Abstractions.IFileInfo> _fileInfoMock)
    {
        Mock<IDirectoryInfo> _directoryInfoMock = new Mock<IDirectoryInfo>();
        _directoryInfoMock.Setup(x => x.Exists).Returns(isDirectoryExisting);
        _directoryInfoMock.Setup(x => x.Create()).Verifiable();
        _directoryInfoMock.Setup(x => x.Delete()).Verifiable();
        _directoryInfoMock.Setup(x => x.MoveTo(directoryPath)).Verifiable();
        _fileInfoMock.Setup(x => x.Delete()).Verifiable();
        _directoryInfoMock.Setup(x => x.GetFiles()).Returns(new System.IO.Abstractions.IFileInfo[] { _fileInfoMock.Object });
        return _directoryInfoMock;
    }
}
