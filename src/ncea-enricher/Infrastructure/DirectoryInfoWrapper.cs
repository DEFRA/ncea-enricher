using Ncea.Enricher.Infrastructure.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class DirectoryInfoWrapper : IDirectoryInfoWrapper
    {
        public DirectoryInfoWrapper GetDirectoryInfo(string dirPath)
        {
            DirectoryPath = dirPath;
            DirectoryInfo = new DirectoryInfo(DirectoryPath);
            return this;
        }
        public string DirectoryPath { get; set; } = null!;
        public DirectoryInfo DirectoryInfo { get; set; } = null!;

        public bool Exists => DirectoryInfo.Exists;

        public string Extension => DirectoryInfo.Extension;

        public string FullName => DirectoryInfo.FullName;

        public string Name => DirectoryInfo.Name;

        public void Create()
        {
            DirectoryInfo.Create();
        }

        public void Delete()
        {
            DirectoryInfo.Delete();
        }

        public FileInfo[] GetFiles()
        {
            return DirectoryInfo.GetFiles();
        }

        public void MoveTo(string destDirName)
        {
            DirectoryInfo.MoveTo(destDirName);
        }
    }
}
