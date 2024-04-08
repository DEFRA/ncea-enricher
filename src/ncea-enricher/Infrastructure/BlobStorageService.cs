using Azure.Storage.Blobs;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
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
        var items = new List<Classifier>();
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

            var regEx = new Regex(@"L([0-9]+)\ ID");

            var levels = dtData.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .Where(x => regEx.IsMatch(x))
                .ToList()
                .SelectMany(y => Regex.Split(y, @"\D+"))
                .Select(z => int.Parse(z));

            items.Add(new Classifier { Id = "lvl0 - 000", Level = 0 });
            foreach (DataRow row in dtData.Rows)
            {
                foreach (var level in levels)
                {
                    if(row[$"L{level} ID"] != null && row[$"L{level} Term"] != null)
                    {
                        var classifier = new Classifier
                        {
                            ParentId = level == 1 ? "lvl0 - 000" : row[$"L{level - 1} ID"].ToString()!,
                            Id = row[$"L{level} ID"].ToString()!,
                            Level = level,
                            Name = row[$"L{level} Term"].ToString()!,
                            Synonyms = (row[$"L{level} Synonyms"] != null) ? row[$"L{level} Synonyms"].ToString()!.Split("").ToList() : null
                        };
                        items.Add(classifier);
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