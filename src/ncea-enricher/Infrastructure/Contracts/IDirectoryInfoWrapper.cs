namespace Ncea.Enricher.Infrastructure.Contracts
{
    public interface IDirectoryInfoWrapper
    {
        DirectoryInfo DirectoryInfo { get; set; }
        string DirectoryPath { get; set; }
        bool Exists { get; }
        string Extension { get; }
        string FullName { get; }
        string Name { get; }

        void Create();
        void Delete();
        DirectoryInfoWrapper GetDirectoryInfo(string dirPath);
        FileInfo[] GetFiles();
        void MoveTo(string destDirName);
    }
}