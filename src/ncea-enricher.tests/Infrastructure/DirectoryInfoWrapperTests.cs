using Moq;
using Xunit;
using System.IO;
using Ncea.Enricher.Infrastructure;
using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Enricher.Tests.Infrastructure
{
    public class DirectoryInfoWrapperTests
    {
        private DirectoryInfo _directoryInfo;
        private readonly string _dirPath = Path.Combine(Directory.GetCurrentDirectory(), "DirectoryInfoWrapper_Test_Direcory");

        public DirectoryInfoWrapperTests()
        {
            _directoryInfo = new DirectoryInfo(_dirPath);
        }

        [Fact]
        public void GetDirectoryInfo_ShouldSetDirectoryPathAndDirectoryInfo()
        {
            // Arrange
            _directoryInfo.Create();
            var dirInfoWrapper = new CustomDirectoryInfoWrapper().GetDirectoryInfo(_dirPath);

            // Act
            // Assert
            Assert.Equal(_directoryInfo.Exists, dirInfoWrapper.Exists);
            Assert.Equal(_directoryInfo.Exists, dirInfoWrapper.DirectoryInfo.Exists);
            Assert.Equal(_directoryInfo.Extension, dirInfoWrapper.DirectoryInfo.Extension);
            Assert.Equal(_directoryInfo.FullName, dirInfoWrapper.DirectoryInfo.FullName);
            Assert.Equal(_directoryInfo.Name, dirInfoWrapper.DirectoryInfo.Name);

            //Cleanup
            _directoryInfo.Delete();

        }
    }
}
