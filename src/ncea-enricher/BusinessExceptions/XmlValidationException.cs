using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.BusinessExceptions;

[ExcludeFromCodeCoverageAttribute]
public class XmlValidationException : BusinessException
{
    public XmlValidationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
