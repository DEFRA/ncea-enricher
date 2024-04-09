using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processors.Contracts;
using System.Data;
using System.Text.RegularExpressions;

namespace Ncea.Enricher.Processor;

public class SynonymsProvider : ISynonymsProvider
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly IBlobStorageService _blobStorageService;

    public SynonymsProvider(IConfiguration configuration, IMemoryCache memoryCache, IBlobStorageService blobStorageService)
    {
        _configuration = configuration;
        _memoryCache = memoryCache;
        _blobStorageService = blobStorageService;
    }

    public async Task<Classifiers> GetAll(CancellationToken cancellationToken)
    {
        var synonymsContainerName = _configuration.GetValue<string>("SynonymsContainerName");
        var synonymsFileName = _configuration.GetValue<string>("SynonymsFileName");
        var rawData = await _blobStorageService.ReadCsvFileAsync(synonymsContainerName!, synonymsFileName!, cancellationToken);        

        return new Classifiers
        {
            NceaClassifiers = GetClassifiers(rawData)
        };
    }

    private List<Classifier> GetClassifiers(DataTable rawData)
    {
        var regEx = new Regex(@"L([0-9]+)\ ID");

        var levels = rawData.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .Where(x => regEx.IsMatch(x))
            .ToList()
            .SelectMany(y => Regex.Split(y, @"\D+"))
            .Select(z => int.Parse(z));

        var items = new HashSet<Classifier>
        {
            new Classifier { Id = "lvl0 - 000", Level = 0, Name = string.Empty }
        };
        foreach (DataRow row in rawData.Rows)
        {
            foreach (var level in levels)
            {
                if (row[$"L{level} ID"] != null && row[$"L{level} Term"] != null)
                {
                    var classifier = new Classifier
                    {
                        ParentId = level == 1 ? "lvl0 - 000" : row[$"L{level - 1} ID"].ToString()!.Trim(),
                        Id = row[$"L{level} ID"].ToString()!.Trim(),
                        Level = level,
                        Name = row[$"L{level} Term"].ToString()!.Trim(),
                        Synonyms = (row[$"L{level} Synonyms"] != null) ? row[$"L{level} Synonyms"].ToString()!.Trim().Split("##").ToList() : null
                    };
                    items.Add(classifier);
                }
            }
        }

        return items.ToList();
    }
}
