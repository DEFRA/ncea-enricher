
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.BusinessExceptions;

[ExcludeFromCodeCoverageAttribute]
public class FileShareNotFoundException : BusinessException
{
    public FileShareNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
