﻿using Microsoft.Extensions.DependencyInjection;
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
    public async Task GivenPredictTheme_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme();

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputValuesAreNotNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme()
        {
            Title = "test-title",
            Abstract = "test-abstract",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle",
            Theme = "test-theme"
        };

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

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
    public void GivenPredictCategory_WhenInputValuesAreNotNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputCategory()
        {
            Title = "test-title",
            Abstract = "test-abstract",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle",
            Theme = "test-theme",
            CategoryL2 = "test-category"
        };

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

    [Fact]
    public void GivenPredictSubCategory_WhenInputValuesAreNotNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputSubCategory()
        {
            Title = "test-title",
            Abstract = "test-abstract",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle",
            Theme = "test-theme",
            CategoryL2 = "test-category",
            SubCategoryL3 = "test-sub-category"
        };

        //Act
        var result = _classifierPredictionService.PredictSubCategory(TrainedModels.SubCategory, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }
}
