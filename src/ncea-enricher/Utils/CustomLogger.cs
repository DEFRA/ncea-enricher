namespace Ncea.Enricher.Utils;

public static class CustomLogger
{
    public static void LogErrorMessage(ILogger logger, string errorMessage, Exception? exception)
    {
#pragma warning disable CA2254 // Template should be a static expression
        logger.LogError(exception, errorMessage);
#pragma warning restore CA2254 // Template should be a static expression
    }
}
