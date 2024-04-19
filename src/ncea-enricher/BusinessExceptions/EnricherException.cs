namespace Ncea.Enricher.BusinessExceptions
{
    public class EnricherException : BusinessException
    {
        public EnricherException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
