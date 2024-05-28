namespace Ncea.Enricher.Infrastructure.Models.Requests;

public class DeleteBlobRequest
{
    public DeleteBlobRequest(string fileName, string container) =>
    (FileName, Container) = (fileName, container);

    public string FileName { get; set; }

    public string Container { get; set; }
}

