using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher.BusinessExceptions
{
    [ExcludeFromCodeCoverageAttribute]
    public class EnricherException : BusinessException
    {
        public EnricherException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
