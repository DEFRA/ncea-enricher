using Moq;
using Xunit;
using System.IO;
using Ncea.Enricher.Infrastructure;
using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Enricher.Tests.Infrastructure
{
    public class DirectoryInfoWrapperTests
    {
        private Mock<DirectoryInfoWrapper> _directoryInfoWrapperMock;

        public DirectoryInfoWrapperTests()
        {
            _directoryInfoWrapperMock = new Mock<DirectoryInfoWrapper>();
        }

        [Fact]
        public void GetDirectoryInfo_ShouldSetDirectoryPathAndDirectoryInfo()
        {
            // Arrange
            var result = true;
            var dirInfo = new DirectoryInfo(It.IsAny<string>());
            _directoryInfoWrapperMock.Setup(x => x.DirectoryInfo).Returns(dirInfo);
            _directoryInfoWrapperMock.Setup(x => x.DirectoryInfo.Exists).Returns(true);

            // Act
            // Assert
            Assert.Equal(result, _directoryInfoWrapperMock.Object.Exists);
        }

        //[Fact]
        //public void Exists_ShouldReturnDirectoryExists()
        //{
        //    // Arrange
        //    _mockDirectoryInfo.Setup(d => d.Exists).Returns(true);
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    var result = _directoryInfoWrapper.Exists;

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public void Extension_ShouldReturnDirectoryExtension()
        //{
        //    // Arrange
        //    var extension = ".ext";
        //    _mockDirectoryInfo.Setup(d => d.Extension).Returns(extension);
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    var result = _directoryInfoWrapper.Extension;

        //    // Assert
        //    Assert.Equal(extension, result);
        //}

        //[Fact]
        //public void FullName_ShouldReturnDirectoryFullName()
        //{
        //    // Arrange
        //    var fullName = "fullName";
        //    _mockDirectoryInfo.Setup(d => d.FullName).Returns(fullName);
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    var result = _directoryInfoWrapper.FullName;

        //    // Assert
        //    Assert.Equal(fullName, result);
        //}

        //[Fact]
        //public void Name_ShouldReturnDirectoryName()
        //{
        //    // Arrange
        //    var name = "name";
        //    _mockDirectoryInfo.Setup(d => d.Name).Returns(name);
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    var result = _directoryInfoWrapper.Name;

        //    // Assert
        //    Assert.Equal(name, result);
        //}

        //[Fact]
        //public void Create_ShouldCallCreateOnDirectoryInfo()
        //{
        //    // Arrange
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    _directoryInfoWrapper.Create();

        //    // Assert
        //    _mockDirectoryInfo.Verify(d => d.Create(), Times.Once);
        //}

        //[Fact]
        //public void Delete_ShouldCallDeleteOnDirectoryInfo()
        //{
        //    // Arrange
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    _directoryInfoWrapper.Delete();

        //    // Assert
        //    _mockDirectoryInfo.Verify(d => d.Delete(), Times.Once);
        //}

        //[Fact]
        //public void GetFiles_ShouldReturnFilesFromDirectoryInfo()
        //{
        //    // Arrange
        //    var files = new FileInfo[] { new FileInfo("file1"), new FileInfo("file2") };
        //    _mockDirectoryInfo.Setup(d => d.GetFiles()).Returns(files);
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    var result = _directoryInfoWrapper.GetFiles();

        //    // Assert
        //    Assert.Equal(files, result);
        //}

        //[Fact]
        //public void MoveTo_ShouldCallMoveToOnDirectoryInfo()
        //{
        //    // Arrange
        //    var destDirName = "destDir";
        //    _directoryInfoWrapper.DirectoryInfo = _mockDirectoryInfo.Object;

        //    // Act
        //    _directoryInfoWrapper.MoveTo(destDirName);

        //    // Assert
        //    _mockDirectoryInfo.Verify(d => d.MoveTo(destDirName), Times.Once);
        //}
    }
}
