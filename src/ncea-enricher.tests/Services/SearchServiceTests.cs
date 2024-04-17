using Ncea.Enricher.Services;
using FluentAssertions;

namespace Ncea.Enricher.Tests.Services;

public class SearchServiceTests
{
    [Fact]
    public void IsMatchFound_WhenCheckingMatchForFieldValue_ShouldReturnTrue()
    {
        //Arrange
        var searchService = new SearchService();
        var synonyms = new List<string>
        {
            "medin",
            "Examples"
        };
        var xmlFieldValue = "Demonstration XML resource for series showing examples of good practice for MEDIN metadata creation";

        // Act
        var result = searchService.IsMatchFound(xmlFieldValue, synonyms);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatchFound_WhenCheckingMatchForFieldValues_ShouldReturnTrue()
    {
        //Arrange
        var searchService = new SearchService();
        var synonyms = new List<string>
        {
            "medin",
            "Examples"
        };
        var xmlFieldValue1 = "Demonstration XML resource for series showing examples of good practice for MEDIN metadata creation";
        var xmlFieldValue2 = "Test, A, B, C";

        var fieldValues = new List<string>
        {
            xmlFieldValue1, xmlFieldValue2
        };

        // Act
        var result = searchService.IsMatchFound(fieldValues, synonyms);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatchFound_IsMatchFound_WhenCheckingMatchForFieldValue_ShouldReturnFalse()
    {
        //Arrange
        var searchService = new SearchService();
        var synonyms = new List<string>
        {
            "medin1",
            "Example"
        };
        var xmlFieldValue = "Demonstration XML resource for series showing examples of good practice for MEDIN metadata creation";

        // Act
        var result = searchService.IsMatchFound(xmlFieldValue, synonyms);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatchFound_IsMatchFound_WhenCheckingMatchForFieldValues_ShouldReturnFalse()
    {
        //Arrange
        var searchService = new SearchService();
        var synonyms = new List<string>
        {
            "medin",
            "Examples"
        };
        var xmlFieldValue1 = "Test, 1, 2, 3";
        var xmlFieldValue2 = "Test, A, B, C";

        var fieldValues = new List<string>
        {
            xmlFieldValue1, xmlFieldValue2
        };

        // Act
        var result = searchService.IsMatchFound(fieldValues, synonyms);

        // Assert
        result.Should().BeFalse();
    }


}
