namespace Ncea.Enricher.Infrastructure.Contracts
{
    public interface IDirectoryInfoWrapper
    {
        DirectoryInfoWrapper GetDirectoryInfo(string dirPath);
        string DirectoryPath { get; set; }
        DirectoryInfo DirectoryInfo { get; set; }
        FileAttributes Attributes { get; set; }
        DateTime CreationTime { get; set; }
        DateTime CreationTimeUtc { get; set; }
        bool Exists { get; }
        string Extension { get; }
        string FullName { get; }
        DateTime LastAccessTime { get; set; }
        DateTime LastAccessTimeUtc { get; set; }
        DateTime LastWriteTime { get; set; }
        DateTime LastWriteTimeUtc { get; set; }
        string? LinkTarget { get; }
        string Name { get; }
        DirectoryInfo? Parent { get; }
        DirectoryInfo Root { get; }
        UnixFileMode UnixFileMode { get; set; }

        void Create();
        void CreateAsSymbolicLink(string pathToTarget);
        DirectoryInfo CreateSubdirectory(string path);
        void Delete();
        void Delete(bool recursive);
        IEnumerable<DirectoryInfo> EnumerateDirectories();
        IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern);
        IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, EnumerationOptions enumerationOptions);
        IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption);
        IEnumerable<FileInfo> EnumerateFiles();
        IEnumerable<FileInfo> EnumerateFiles(string searchPattern);
        IEnumerable<FileInfo> EnumerateFiles(string searchPattern, EnumerationOptions enumerationOptions);
        IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption);
        IEnumerable<FileSystemInfo> EnumerateFileSystemInfos();
        IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern);
        IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions);
        IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption);
        DirectoryInfo[] GetDirectories();
        DirectoryInfo[] GetDirectories(string searchPattern);
        DirectoryInfo[] GetDirectories(string searchPattern, EnumerationOptions enumerationOptions);
        DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption);
        FileInfo[] GetFiles();
        FileInfo[] GetFiles(string searchPattern);
        FileInfo[] GetFiles(string searchPattern, EnumerationOptions enumerationOptions);
        FileInfo[] GetFiles(string searchPattern, SearchOption searchOption);
        FileSystemInfo[] GetFileSystemInfos();
        FileSystemInfo[] GetFileSystemInfos(string searchPattern);
        FileSystemInfo[] GetFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions);
        FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption);
        void MoveTo(string destDirName);
        void Refresh();
        FileSystemInfo? ResolveLinkTarget(bool returnFinalTarget);
    }
}