
namespace Ncea.Enricher.BusinessExceptions;

public class FileShareNotFoundException : BusinessException
{
    public FileShareNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
