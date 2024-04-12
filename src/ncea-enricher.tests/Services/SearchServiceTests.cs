using Ncea.Enricher.Services;
using FluentAssertions;

namespace Ncea.Enricher.Tests.Services;

public class SearchServiceTests
{
    [Fact]
    public void IsMatchFound_ShouldReturnTrue()
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
    public void IsMatchFound_ShouldReturnFalse()
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
}
