
namespace Ncea.Enricher.BusinessExceptions;

public class SynonymsNotAccessibleException : BusinessException
{
    public SynonymsNotAccessibleException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
