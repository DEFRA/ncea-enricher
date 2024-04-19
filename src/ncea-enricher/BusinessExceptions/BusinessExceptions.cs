namespace Ncea.Enricher.BusinessExceptions;

public abstract class BusinessException : Exception
{
    protected BusinessException()
    {
    }

    protected BusinessException(string message)
        : base(message)
    {
    }

    protected BusinessException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
