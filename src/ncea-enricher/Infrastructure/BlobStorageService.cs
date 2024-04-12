using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ncea.Enricher.Infrastructure.Contracts;
using OfficeOpenXml;
using System.Data;

namespace Ncea.Enricher.Infrastructure;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient) =>
        _blobServiceClient = blobServiceClient;

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