﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using OfficeOpenXml;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;

namespace Ncea.Enricher.Infrastructure;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient) =>
        (_blobServiceClient) = (blobServiceClient);

    public async Task<DataTable> ReadCsvFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
    {        
        var dtData = new DataTable();
        try
        {
            var rows = new List<string>();
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainer.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                using (var streamReader = new StreamReader(response.Value.Content))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = await streamReader.ReadLineAsync(cancellationToken);
                        rows.Add(line!);
                    }
                }
            }
            if (rows.Count > 0)
            {
                foreach (string columnName in rows[0].Split(';'))
                    dtData.Columns.Add(columnName);
            }

            for (int row = 1; row < rows.Count; row++)
            {
                var rowValues = rows[row].Split(';');
                var dr = dtData.NewRow();
                dr.ItemArray = rowValues;
                dtData.Rows.Add(dr);
            }            
        }
        catch (Exception ex) 
        {
            Console.WriteLine("\t" + ex.Message);
        }               
        return dtData;
    }

    public async Task<DataTable> ReadExcelFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
    {
        var dtData = new DataTable();
        try
        {
            var rows = new List<string>();
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainer.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var stream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(true)))
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["ncea-classifiers"];
                        dtData = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable(c =>
                        {
                            c.FirstRowIsColumnNames = true;
                        });
                    }
                }
            }            
        }
        catch (Exception ex)
        {
            Console.WriteLine("\t" + ex.Message);
        }
        return dtData;
    }
}