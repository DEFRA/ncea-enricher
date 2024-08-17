using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Enricher.Infrastructure
{
    public class DirectoryInfoWrapper : IDirectoryInfoWrapper
    {
        public DirectoryInfoWrapper GetDirectoryInfo(string dirPath)
        {
            DirectoryPath = dirPath;
            DirectoryInfo = new DirectoryInfo(DirectoryPath);
            Attributes = DirectoryInfo.Attributes;
            CreationTime = DirectoryInfo.CreationTime;
            CreationTimeUtc = DirectoryInfo.CreationTimeUtc;
            LastAccessTime = DirectoryInfo.LastAccessTime;
            LastAccessTimeUtc = DirectoryInfo.LastAccessTimeUtc;
            LastWriteTime = DirectoryInfo.LastWriteTime;
            LastWriteTimeUtc = DirectoryInfo.LastWriteTimeUtc;
            return this;
        }
        public string DirectoryPath { get; set; } = null!;
        public DirectoryInfo DirectoryInfo { get; set; } = null!;

        public DirectoryInfo? Parent => DirectoryInfo.Parent;

        public DirectoryInfo Root => DirectoryInfo.Root;

        public FileAttributes Attributes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime CreationTimeUtc { get; set; }

        public bool Exists => DirectoryInfo.Exists;

        public string Extension => DirectoryInfo.Extension;

        public string FullName => DirectoryInfo.FullName;

        public DateTime LastAccessTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public string? LinkTarget => DirectoryInfo.LinkTarget;

        public string Name => DirectoryInfo.Name;

        public UnixFileMode UnixFileMode { get; set; }

        public void Create()
        {
            DirectoryInfo.Create();
        }

        public void CreateAsSymbolicLink(string pathToTarget)
        {
            DirectoryInfo.CreateAsSymbolicLink(pathToTarget);
        }

        public DirectoryInfo CreateSubdirectory(string path)
        {
            return DirectoryInfo.CreateSubdirectory(path);
        }

        public void Delete(bool recursive)
        {
            DirectoryInfo.Delete(recursive);
        }

        public void Delete()
        {
            DirectoryInfo.Delete();
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories()
        {
            return DirectoryInfo.EnumerateDirectories();
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return DirectoryInfo.EnumerateDirectories(searchPattern);
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            return DirectoryInfo.EnumerateDirectories(searchPattern, searchOption);
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return DirectoryInfo.EnumerateDirectories(searchPattern, enumerationOptions);
        }

        public IEnumerable<FileInfo> EnumerateFiles()
        {
            return DirectoryInfo.EnumerateFiles();
        }

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
        {
            return DirectoryInfo.EnumerateFiles(searchPattern);
        }

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            return DirectoryInfo.EnumerateFiles(searchPattern, searchOption);
        }

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return DirectoryInfo.EnumerateFiles(searchPattern, enumerationOptions);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
        {
            return DirectoryInfo.EnumerateFileSystemInfos();
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return DirectoryInfo.EnumerateFileSystemInfos(searchPattern);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return DirectoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return DirectoryInfo.EnumerateFileSystemInfos(searchPattern, enumerationOptions);
        }

        public DirectoryInfo[] GetDirectories()
        {
            return DirectoryInfo.GetDirectories();
        }

        public DirectoryInfo[] GetDirectories(string searchPattern)
        {
            return DirectoryInfo.GetDirectories(searchPattern);
        }

        public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return DirectoryInfo.GetDirectories(searchPattern, searchOption);
        }

        public DirectoryInfo[] GetDirectories(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return DirectoryInfo.GetDirectories(searchPattern, enumerationOptions);
        }

        public FileInfo[] GetFiles()
        {
            return DirectoryInfo.GetFiles();
        }

        public FileInfo[] GetFiles(string searchPattern)
        {
            return DirectoryInfo.GetFiles(searchPattern);
        }

        public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return DirectoryInfo.GetFiles(searchPattern, searchOption);
        }

        public FileInfo[] GetFiles(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return DirectoryInfo.GetFiles(searchPattern, enumerationOptions);
        }

        public FileSystemInfo[] GetFileSystemInfos()
        {
            return DirectoryInfo.GetFileSystemInfos();
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return DirectoryInfo.GetFileSystemInfos(searchPattern);
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return DirectoryInfo.GetFileSystemInfos(searchPattern, searchOption);
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return DirectoryInfo.GetFileSystemInfos(searchPattern, enumerationOptions);
        }

        public void MoveTo(string destDirName)
        {
            DirectoryInfo.MoveTo(destDirName);
        }

        public void Refresh()
        {
            DirectoryInfo.Refresh();
        }

        public FileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
        {
            return DirectoryInfo.ResolveLinkTarget(returnFinalTarget);
        }
    }
}
