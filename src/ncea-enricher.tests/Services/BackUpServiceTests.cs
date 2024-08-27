using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Services;

namespace Ncea.Harvester.Tests.Processors;

public class BackUpServiceTests
{
    private readonly BackUpService _backupService;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly IConfiguration _configuration;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly Mock<ILogger<BackUpService>> _loggerMock;

    private readonly string testFileShare = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "FileShare");
    private readonly string medinDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Medin");
    private readonly string medinNewDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Medin-new");
    private readonly string medinBackupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Medin-backup");

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
        CreateTestFilshare();

        List<KeyValuePair<string, string?>> lstProps =
            [
                new KeyValuePair<string, string?>("FileShareName", testFileShare),
            ];

        _configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();
        _backupService = new BackUpService(_loggerMock.Object);

    }

    private void CreateTestFilshare()
    {
        new DirectoryInfo(testFileShare).Create();
        new DirectoryInfo(medinDirectory).Create();
        new DirectoryInfo(medinNewDirectory).Create();
        new DirectoryInfo(medinBackupDirectory).Create();
    }

    private void DeleteTestFilshare()
    {
        new DirectoryInfo(testFileShare).Delete();
        new DirectoryInfo(medinDirectory).Delete();
        new DirectoryInfo(medinNewDirectory).Delete();
        new DirectoryInfo(medinBackupDirectory).Delete();
    }

    [Fact]
    public void MoveFilesn_WhenSourceDirectory_NotExist_ThrowAnException()
    {
        //Act
        _backupService.MoveFiles(medinDirectory, medinBackupDirectory);

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
        var fileshareDir = new DirectoryInfo(testFileShare);
        fileshareDir.Create();
        var srcDir = new DirectoryInfo(medinDirectory);
        srcDir.Create();


        //Act
        _backupService.MoveFiles(medinDirectory, medinBackupDirectory);

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

        //Cleanup
        fileshareDir.Delete();
        srcDir.Delete();
    }
}
