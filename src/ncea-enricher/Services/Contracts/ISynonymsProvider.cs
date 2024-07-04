namespace Ncea.Enricher.Services.Contracts;

public interface IClassifierVocabularyProvider
{
    Task<List<Models.ClassifierInfo>> GetAll(CancellationToken cancellationToken);
}
