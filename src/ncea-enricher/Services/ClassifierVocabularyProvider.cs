using Ncea.Classifier.Microservice;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class ClassifierVocabularyProvider : IClassifierVocabularyProvider
{
    private readonly INceaClassifierMicroserviceClient _classifierApiClient;

    public ClassifierVocabularyProvider(INceaClassifierMicroserviceClient classifierApiClient)
    {
        _classifierApiClient = classifierApiClient;
    }

    public async Task<List<ClassifierInfo>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _classifierApiClient.VocabularyAsync(cancellationToken);
    }
}
