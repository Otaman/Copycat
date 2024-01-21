using Copycat.IntegrationTests.Core;

namespace Copycat.IntegrationTests;

[Decorate]
public partial class CallLoggerWithRemoteBase : RemoteBase, IChattyInterface
{
    [Template]
    private string LogMessage(Func<string, string> action, string message)
    {
        var result = action(message);
        Log.Add($"Q: {message} - A: {result}");
        return result;
    }
}