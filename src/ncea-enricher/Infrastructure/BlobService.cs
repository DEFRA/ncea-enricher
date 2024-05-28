using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Infrastructure.Models.Requests;
using OfficeOpenXml;
using System.Data;
using System.Text;

namespace Ncea.Enricher.Infrastructure;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(BlobServiceClient blobServiceClient) =>
        _blobServiceClient = blobServiceClient;

    public async Task<string> GetContentAsync(GetBlobContentRequest request, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(request.Container);

        var blobClient = containerClient.GetBlobClient(request.FileName);
        var response = await blobClient.DownloadContentAsync(cancellationToken);

        var data = response.Value.Content;
        var blobContents = Encoding.UTF8.GetString(data);

        return blobContents;
    }

    public async Task DeleteBlobAsync(DeleteBlobRequest request, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(request.Container);
        var blobClient = containerClient.GetBlobClient(request.FileName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, cancellationToken);
    }

    public async Task<DataTable> ReadExcelFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
    {
        var dtData = new DataTable();

        var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainer.GetBlobClient(fileName);
        if (await blobClient.ExistsAsync(cancellationToken))
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(true), cancellationToken))
            {
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets["ncea-classifiers"];
                    dtData = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable(c =>
                    {
                        c.FirstRowIsColumnNames = true;
                    });
                }
            }
        }

        return dtData;
    }
}