namespace Copycat.IntegrationTests;

public interface IProvideManyThings
{
    bool GetBool();
    int GetInt();
    string GetString();
    
    Task<bool> GetBoolAsync();
    Task<int> GetIntAsync();
    Task<string> GetStringAsync();
}

[Decorate]
public partial class JustLog : IProvideManyThings
{
    [Template]
    private T Log<T>(Func<T> action)
    {
        Console.WriteLine($"Calling {nameof(action)}");
        return action();
    }
    
    [Template]
    private async Task<T> LogAsync<T>(Func<Task<T>> action)
    {
        Console.WriteLine($"Calling async {nameof(action)}");
        return await action();
    }
}