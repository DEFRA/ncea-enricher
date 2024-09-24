using FluentAssertions;
using Ncea.Enricher.Models.ML;

namespace Ncea.Enricher.Tests.Models;

public class PredictedHierarchyTests
{
    [Fact]
    public void PredictedHierarchyEquals_ShouldBeTrue()
    {
        //Arrange
        var predictedHierarchy = new PredictedHierarchy("test-theme", "test-theme-code", "test-category", "test-category-code", "test-subcategory");

        //Act
        var result = predictedHierarchy.Equals(predictedHierarchy);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PredictedHierarchyObjectEquals_ShouldBeTrue()
    {
        //Arrange
        var predictedHierarchy = new PredictedHierarchy("test-theme", "test-theme-code", "test-category", "test-category-code", "test-subcategory");

        //Act
        var result = predictedHierarchy.Equals((object)predictedHierarchy);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PredictedHierarchyEquals_ShouldBeFalse()
    {
        //Arrange
        var predictedHierarchy = new PredictedHierarchy("test-theme", "test-theme-code", "test-category", "test-category-code", "test-subcategory");

        //Act
        var result = predictedHierarchy.Equals(null);

        //Assert
        result.Should().BeFalse();
    }
}
