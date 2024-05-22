namespace Ncea.Enricher.Infrastructure.Models.Requests;

public class GetBlobContentRequest
{
    public GetBlobContentRequest(string fileName, string container) =>
    (FileName, Container) = (fileName, container);

    public string FileName { get; set; }

    public string Container { get; set; }
}
