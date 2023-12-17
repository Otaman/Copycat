namespace Copycat.IntegrationTests;

public interface IReporter
{
    Task Report(string message);
    Task Report(string message, Exception exception);
}

[Decorate]
public partial class TaskWrapper : IReporter
{
    private readonly IReporter _reporter;

    public TaskWrapper(IReporter reporter) => _reporter = reporter;

    [Template]
    public async Task WrapTask(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception e)
        {
            await _reporter.Report("Failed to execute action", e);
        }
    }
}