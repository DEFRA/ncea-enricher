using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using Ncea.Enricher.Models.ML;
using FluentAssertions;

namespace Ncea.Enricher.Tests.Services;

public class ClassifierPredictionServiceTests
{
    private IServiceProvider _serviceProvider;
    private IClassifierPredictionService _classifierPredictionService;

    public ClassifierPredictionServiceTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _classifierPredictionService = _serviceProvider.GetService<IClassifierPredictionService>()!;
    }

    [Fact]
    public void GivenPredictTheme_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme();

        //Act
        var result = _classifierPredictionService.PredictTheme(TrainedModels.Theme, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public void GivenPredictCategory_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputCategory();

        //Act
        var result = _classifierPredictionService.PredictCategory(TrainedModels.Category, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public void GivenPredictSubCategory_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputSubCategory();

        //Act
        var result = _classifierPredictionService.PredictSubCategory(TrainedModels.SubCategory, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }
}
