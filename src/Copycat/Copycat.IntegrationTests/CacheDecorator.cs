namespace Copycat.IntegrationTests;

public interface ICache<T>
{
    Task<T> Get(string key);
    Task<T> Set(string key, T value);
}

[Decorate]
public partial class CacheDecorator<T> : ICache<T>
{
    private readonly ICache<T> _decorated;
    
    public CacheDecorator(ICache<T> decorated) => _decorated = decorated;
    
    [Template]
    public async Task<T> RetryOnce(Func<Task<T>> action, string key)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Retry {nameof(action)} for {key} due to {e.Message}");
            return await action();
        }
    }
}