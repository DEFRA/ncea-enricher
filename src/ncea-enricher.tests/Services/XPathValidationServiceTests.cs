﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Xml.Linq;

namespace Ncea.Enricher.Tests.Services;

public class XPathValidationServiceTests
{
    private readonly IXmlValidationService _xpathValidationService;
    
    public XPathValidationServiceTests()
    {
        var serviceProvider = ServiceProviderForTests.Get();
        var configuration = serviceProvider!.GetService<IConfiguration>()!;
        _xpathValidationService = new XPathValidationService(configuration);
    }

    [Fact]
    public void Validate_WhenTheGivenXmlIsValid_ThenNoExceptionThrown()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = XDocument.Load(filePath);

        //Act
        _xpathValidationService.Validate(xDoc!);

        //Assert
    }
}
