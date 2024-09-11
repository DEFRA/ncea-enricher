using FluentAssertions;
using Ncea.Enricher.Extensions;
using Ncea.Enricher.Models.ML;

namespace Ncea.Enricher.Tests.Extensions;

public class ClassifierStringExtensionsTests
{
    [Fact]
    public void GivenGetClassifierIds_WhenPredictedValueIsEmptyString_ThenReturnEmptyArray()
    {
        //Arrange
        string inputString = string.Empty;

        //Act
        var result = ClassifierStringExtensions.GetClassifierIds(inputString);

        //Assert
        result!.Should().BeOfType<PredictedItem[]>();
        result!.Count().Should().Be(0);
    }

    [Fact]
    public void GivenGetClassifierIds_WhenPredictedValueWithSinglePrediction_ThenReturnArrayOfPredictedItem()
    {
        //Arrange
        string inputString = "lvl1-001 Test Classifier-1";

        //Act
        var result = inputString.GetClassifierIds();

        //Assert        
        result!.Count().Should().Be(1);
        result!.First().Code.Should().Be("lvl1-001");
        result!.First().OriginalValue.Should().Be("lvl1-001 Test Classifier-1");
    }

    [Fact]
    public void GivenGetClassifierIds_WhenPredictedValueWithMultiplePredictions_ThenReturnArrayOfPredictedItems()
    {
        //Arrange
        string inputString = "lvl1-001 Test Classifier-1,lvl1-002 Test Classifier-2";

        //Act
        var result = inputString.GetClassifierIds();

        //Assert 
        result!.Count().Should().Be(2);
        result!.First().Code.Should().Be("lvl1-001");
        result!.First().OriginalValue.Should().Be("lvl1-001 Test Classifier-1");
        result!.Last().Code.Should().Be("lvl1-002");
        result!.Last().OriginalValue.Should().Be("lvl1-002 Test Classifier-2");
    }
}
