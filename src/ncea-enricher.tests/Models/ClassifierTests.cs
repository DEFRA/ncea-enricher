using FluentAssertions;
using Ncea.Enricher.Models;

namespace Ncea.Enricher.Tests.Models;

public class ClassifierTests
{
    [Fact]
    public void ClassifierEquals_ShouldBeTrue()
    {
        //Arrange
        var classifier = new ClassifierInfo
        {
            Id = "test-id",
            ParentId = "test-parent-id",
            Name = "test-name"
        };

        //Act
        var result = classifier.Equals(classifier);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ClassifierObjectEquals_ShouldBeTrue()
    {
        //Arrange
        var classifier = new ClassifierInfo
        {
            Id = "test-id",
            ParentId = "test-parent-id",
            Name = "test-name"
        };

        //Act
        var result = classifier.Equals((object)classifier);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ClassifierEquals_ShouldBeFalse()
    {
        //Arrange
        var classifier = new ClassifierInfo
        {
            Id = "test-id",
            ParentId = "test-parent-id",
            Name = "test-name"
        };

        //Act
        var result = classifier.Equals(null);

        //Assert
        result.Should().BeFalse();
    }
}
