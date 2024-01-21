namespace Copycat.IntegrationTests.Core;

public class RemoteBase
{
    protected List<string> Log { get; }

    protected RemoteBase(List<string> log)
    {
        Log = log;
    }
}