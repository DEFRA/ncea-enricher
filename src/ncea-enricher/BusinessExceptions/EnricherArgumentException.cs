
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.BusinessExceptions;

[ExcludeFromCodeCoverageAttribute]
public class EnricherArgumentException : BusinessException
{
    public EnricherArgumentException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
