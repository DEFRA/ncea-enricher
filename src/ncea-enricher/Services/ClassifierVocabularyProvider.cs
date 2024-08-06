using Microsoft.Identity.Abstractions;
using Ncea.Classifier.Microservice.Clients;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class ClassifierVocabularyProvider : IClassifierVocabularyProvider
{
    private readonly IDownstreamApi _downstreamApi;

    public ClassifierVocabularyProvider(IDownstreamApi downstreamApi)
    {
        _downstreamApi = downstreamApi;
    }

    public async Task<List<Models.ClassifierInfo>> GetAll(CancellationToken cancellationToken)
    {
        var classifierList = new List<Models.ClassifierInfo>();
        var classifiers = await _downstreamApi.GetForAppAsync<ICollection<ClassifierInfo>>(serviceName: "ClassifierApi", cancellationToken: cancellationToken);

        foreach(var themeClassifier in classifiers!)
        {
            classifierList.Add(new Models.ClassifierInfo() { Id = themeClassifier.Code, Name = themeClassifier.Name, Level = themeClassifier.Level });
            GetChildren(classifierList, themeClassifier);
        }

        return classifierList;
    }

    private static void GetChildren(List<Models.ClassifierInfo> classifierList, ClassifierInfo parentClassifier)
    {
        if(parentClassifier.Classifiers != null)
        {
            foreach (var classifier in parentClassifier.Classifiers)
            {
                classifierList.Add(new Models.ClassifierInfo() { Id = classifier.Code, Name = classifier.Name, Level = classifier.Level, ParentId = parentClassifier.Code });
                GetChildren(classifierList, classifier);
            }
        }
    }
}
