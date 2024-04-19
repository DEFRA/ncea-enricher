
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.BusinessExceptions;

[ExcludeFromCodeCoverageAttribute]
public class SynonymsNotAccessibleException : BusinessException
{
    public SynonymsNotAccessibleException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
