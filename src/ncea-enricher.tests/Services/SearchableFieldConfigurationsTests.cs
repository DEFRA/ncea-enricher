using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services;
using Ncea.Enricher.Tests.Clients;

namespace Ncea.Enricher.Tests.Services;

public class SearchableFieldConfigurationsTests
{
    private IServiceProvider _serviceProvider;

    public SearchableFieldConfigurationsTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
    }

    [Fact]
    public void GetSearchableFieldConfigurations_WhenAppSettinsHasSearchableFieldConfigurations_ReturnSerachableFields()
    {
        //Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var searchableFieldConfigurations = new SearchableFieldConfigurations(configuration!);        

        // Act
        var result = searchableFieldConfigurations.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<SearchableField>>();
        result.Should().HaveCount(9);
    }
}
