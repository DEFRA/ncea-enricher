using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;
using System.Data;
using System.Text.RegularExpressions;

namespace Ncea.Enricher.Services;

public class SynonymsProvider : ISynonymsProvider
{
    private readonly IConfiguration _configuration;
    private readonly IBlobStorageService _blobStorageService;

    public SynonymsProvider(IConfiguration configuration, IBlobStorageService blobStorageService)
    {
        _configuration = configuration;
        _blobStorageService = blobStorageService;
    }

    public async Task<List<Classifier>> GetAll(CancellationToken cancellationToken)
    {
        var synonymsContainerName = _configuration.GetValue<string>("SynonymsContainerName");
        var synonymsFileName = _configuration.GetValue<string>("SynonymsFileName");
        var rawData = await _blobStorageService.ReadExcelFileAsync(synonymsContainerName!, synonymsFileName!, cancellationToken);

        return GetClassifiers(rawData);
    }

    private static List<Classifier> GetClassifiers(DataTable rawData)
    {
        var items = new HashSet<Classifier>();

        var regEx = new Regex(@"L([0-9]+)\ ID");

        var levels = rawData.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .Where(x => regEx.IsMatch(x))
            .SelectMany(y => Regex.Split(y, @"\D+"))
            .Where(str => !string.IsNullOrEmpty(str))
            .Select(int.Parse)
            .ToList();

        foreach (DataRow row in rawData.Rows)
        {
            foreach (var level in levels)
            {
                if (row[$"L{level} ID"] != null && row[$"L{level} Term"] != null)
                {
                    var classifier = CreateClassifier(rawData, row, level);
                    items.Add(classifier);
                }
            }
        }

        return items
            .OrderBy(x => x.Level)
            .ThenBy(x => x.ParentId)
            .ThenBy(x => x.Id)
            .ToList();
    }

    private static Classifier CreateClassifier(DataTable rawData, DataRow row, int level)
    {
        var isSynonymColumnExists = rawData.Columns.Contains($"L{level} Synonyms");

        var classifier = new Classifier
        {
            ParentId = level == 1 ? null : row[$"L{level - 1} ID"].ToString()!.Trim(),
            Id = row[$"L{level} ID"].ToString()!.Trim(),
            Level = level,
            Name = row[$"L{level} Term"].ToString()!.Trim()
        };

        if (isSynonymColumnExists)
        {
            var synonyms = row[$"L{level} Synonyms"].ToString()!.Trim();
            classifier.Synonyms = !string.IsNullOrWhiteSpace(synonyms) ? synonyms.Trim().Split("##").ToList() : null;
        }

        return classifier;
    }
}
