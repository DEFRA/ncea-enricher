using Ncea.Classifier.Microservice;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class ClassifierVocabularyProvider : IClassifierVocabularyProvider
{
    private readonly string _apiKey;
    private readonly INceaClassifierMicroserviceClient _classifierApiClient;

    public ClassifierVocabularyProvider(INceaClassifierMicroserviceClient classifierApiClient, IConfiguration configuration)
    {
        _apiKey = configuration.GetValue<string>(ApiKeyParameters.ApiKeyName)!;
        _classifierApiClient = classifierApiClient;
    }

    public async Task<List<Models.ClassifierInfo>> GetAll(CancellationToken cancellationToken)
    {
        var classifierList = new List<Models.ClassifierInfo>();
        var classifiers = await _classifierApiClient.VocabularyAsync(_apiKey, cancellationToken);

        foreach(var themeClassifier in classifiers)
        {
            classifierList.Add(new Models.ClassifierInfo() { Id = themeClassifier.Code, Name = themeClassifier.Name, Level = themeClassifier.Level });
            GetChildren(classifierList, themeClassifier);
        }

        return classifierList;
    }

    private static void GetChildren(List<Models.ClassifierInfo> classifierList, Classifier.Microservice.ClassifierInfo parentClassifier)
    {
        foreach (var classifier in parentClassifier.Classifiers)
        {
            classifierList.Add(new Models.ClassifierInfo() { Id = classifier.Code, Name = classifier.Name, Level = classifier.Level, ParentId = parentClassifier.Code });
            GetChildren(classifierList, classifier);
        }
    }
}
