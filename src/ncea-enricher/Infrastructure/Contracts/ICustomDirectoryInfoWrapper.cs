namespace Ncea.Enricher.Infrastructure.Contracts
{
    public interface ICustomDirectoryInfoWrapper
    {
        DirectoryInfo DirectoryInfo { get; set; }
        string DirectoryPath { get; set; }
        bool Exists { get; }
        string Extension { get; }
        string FullName { get; }
        string Name { get; }

        void Create();
        void Delete();
        CustomDirectoryInfoWrapper GetDirectoryInfo(string dirPath);
        FileInfo[] GetFiles();
        void MoveTo(string destDirName);
    }
}