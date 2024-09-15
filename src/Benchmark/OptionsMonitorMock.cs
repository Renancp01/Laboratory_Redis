using Microsoft.Extensions.Options;
using Moq;

namespace Benchmark;

public class OptionsMonitorMock
{
    public static IOptionsMonitor<T> Create<T>(T options) where T : class, new()
    {
        var mock = new Mock<IOptionsMonitor<T>>();
        mock.Setup(m => m.CurrentValue).Returns(options);
        return mock.Object;
    }
}