using Moq;
using Microsoft.Extensions.Logging;

namespace Ncea.Enricher.Tests.Clients;

public static class LoggerForTests
{
    public static void Get<T>(out Mock<ILogger<T>> mockLogger){
        mockLogger = new Mock<ILogger<T>>(MockBehavior.Strict);
        mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );
        mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );
    }
}
