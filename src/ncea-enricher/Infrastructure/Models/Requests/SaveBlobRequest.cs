namespace Ncea.Enricher.Infrastructure.Models.Requests;

public class SaveBlobRequest
{
    public SaveBlobRequest(Stream blob, string fileName, string container) =>
        (Blob, FileName, Container) = (blob, fileName, container);

    public Stream Blob { get; set; }

    public string FileName { get; set; }
    
    public string Container { get; set; }
}
