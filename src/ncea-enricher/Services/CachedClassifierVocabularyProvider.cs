using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class CachedClassifierVocabularyProvider : IClassifierVocabularyProvider
{
    private const string ClassifierListCacheKey = "ClassifierVocabularyList";

    private readonly IMemoryCache _memoryCache;
    private readonly IClassifierVocabularyProvider _classifierVocabularyProvider;
    private readonly int _cacheDurationInMinutes = 30;

    public CachedClassifierVocabularyProvider(IMemoryCache memoryCache, IClassifierVocabularyProvider classifierVocabularyProvider, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _classifierVocabularyProvider = classifierVocabularyProvider;
        _cacheDurationInMinutes = configuration.GetValue<int>("CacheDurationInMinutes");
    }

    public async Task<List<ClassifierInfo>> GetAll(CancellationToken cancellationToken)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(_cacheDurationInMinutes));

        if (_memoryCache.TryGetValue(ClassifierListCacheKey, out List<ClassifierInfo>? result)) 
            return result!;

        result = await _classifierVocabularyProvider.GetAll(cancellationToken);

        _memoryCache.Set(ClassifierListCacheKey, result, options);

        return result;
    }
}
