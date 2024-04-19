
namespace Ncea.Enricher.BusinessExceptions;

public class EnricherArgumentException : BusinessException
{
    public EnricherArgumentException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
