using Ncea.Enricher.Tests.Clients;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Tests.Services;

public class ClassifierVocabularyProviderTests
{
    private IServiceProvider _serviceProvider;
    private IClassifierVocabularyProvider _classifierVocabularyProvider;

    public ClassifierVocabularyProviderTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _classifierVocabularyProvider = _serviceProvider.GetService<IClassifierVocabularyProvider>()!;
    }

    [Fact]
    public async Task GivenGetAllClassifierVocabulary_WhenTheServiceIsUp_ThenReturnListOfClassifiers()
    {        
        //Act
        var result = await _classifierVocabularyProvider.GetAll(It.IsAny<CancellationToken>());

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Enricher.Models.ClassifierInfo>>();
        result.Should().HaveCount(135);
    }

    [Fact]
    public async Task GivenGetAllClassifierVocabulary_WhenServedFromCache_ThenReturnListOfClassifiers()
    {
        //Act
        var result = await _classifierVocabularyProvider.GetAll(It.IsAny<CancellationToken>());
        result = await _classifierVocabularyProvider.GetAll(It.IsAny<CancellationToken>());

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Enricher.Models.ClassifierInfo>>();
        result.Should().HaveCount(135);
    }
}
