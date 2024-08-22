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
#pragma warning disable IDE0052 // Remove unread private members
    private readonly IConfiguration _configuration;
#pragma warning restore IDE0052 // Remove unread private members
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
        _backupService = new BackUpService(_loggerMock.Object);
        
    }

    [Fact]
    public void MoveFilesn_WhenSourceDirectory_NotExist_ThrowAnException()
    {
        //Arrange
        var srcDirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        srcDirInfoWrapperMock.Setup(x => x.Exists).Returns(false);
        var trgDirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        trgDirInfoWrapperMock.Setup(x => x.Exists).Returns(false);
        trgDirInfoWrapperMock.Setup(x => x.Create()).Verifiable();

        //Act
        _backupService.MoveFiles(srcDirInfoWrapperMock.Object, trgDirInfoWrapperMock.Object);

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

    [Fact]
    public void MoveFilesn_When_SourceDirectory_Exist_TargetDirectory_NotExist()
    {
        //Arrange
        var srcDirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        srcDirInfoWrapperMock.Setup(x => x.Exists).Returns(true);
        srcDirInfoWrapperMock.Setup(x => x.MoveTo(It.IsNotNull<string>())).Verifiable();
        var trgDirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        trgDirInfoWrapperMock.Setup(x => x.Exists).Returns(false);
        trgDirInfoWrapperMock.Setup(x => x.Create()).Verifiable();


        //Act
        _backupService.MoveFiles(srcDirInfoWrapperMock.Object, trgDirInfoWrapperMock.Object);

        //Assert
        trgDirInfoWrapperMock.Verify(x => x.Create(), Times.Once);
        srcDirInfoWrapperMock.Verify(x => x.MoveTo(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void MoveFilesn_When_SourceDirectory_Exist_TargetDirectory_Exist()
    {
        //Arrange
        var srcDirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        srcDirInfoWrapperMock.Setup(x => x.Exists).Returns(true);
        srcDirInfoWrapperMock.Setup(x => x.GetFiles()).Verifiable();
        srcDirInfoWrapperMock.Setup(x => x.MoveTo(It.IsNotNull<string>())).Verifiable();
        var trgDirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        trgDirInfoWrapperMock.Setup(x => x.Exists).Returns(true);
        trgDirInfoWrapperMock.Setup(x => x.Create()).Verifiable();


        //Act
        _backupService.MoveFiles(srcDirInfoWrapperMock.Object, trgDirInfoWrapperMock.Object);

        //Assert
        trgDirInfoWrapperMock.Verify(x => x.GetFiles(), Times.Once);
        srcDirInfoWrapperMock.Verify(x => x.MoveTo(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void CreateDirectory_When_Directory_NotExist_Create_Once()
    {
        //Arrange
        var dataSourceName = string.Empty;
        var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
        var dirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        dirInfoWrapperMock.Setup(x => x.Exists).Returns(false);
        dirInfoWrapperMock.Setup(x => x.Create()).Verifiable();

        //Act
        _backupService.CreateDirectory(dirInfoWrapperMock.Object);

        //Assert
        dirInfoWrapperMock.Verify(x => x.Create(), Times.Once);
    }

    [Fact]
    public void CreateDirectory_When_Directory_Exist()
    {
        //Arrange
        var dataSourceName = string.Empty;
        var _fileInfoMock = new Mock<System.IO.Abstractions.IFileInfo>();
        var dirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        dirInfoWrapperMock.Setup(x => x.Exists).Returns(true);
        dirInfoWrapperMock.Setup(x => x.Create()).Verifiable();

        //Act
        _backupService.CreateDirectory(dirInfoWrapperMock.Object);

        //Assert
        dirInfoWrapperMock.Verify(x => x.Exists, Times.Once);
        dirInfoWrapperMock.Verify(x => x.Create(), Times.Never);
    }

    [Fact]
    public void CreateDirectory_When_DirectoryName_IsNull()
    {
        //Arrange
        var dirInfoWrapperMock = new Mock<ICustomDirectoryInfoWrapper>();
        dirInfoWrapperMock.Setup(x => x.Exists).Returns(false);
        dirInfoWrapperMock.Setup(x => x.Create()).Throws<Exception>();

        //Act
        _backupService.CreateDirectory(dirInfoWrapperMock.Object);

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
}
