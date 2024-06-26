﻿using System.Xml.Linq;

namespace Ncea.Enricher.Services.Contracts;

public interface IXmlValidationService
{
    void Validate(XDocument xDoc, string dataSource, string fileIdentifier);
}
