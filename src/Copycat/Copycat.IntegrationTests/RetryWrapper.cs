namespace Copycat.IntegrationTests;

public interface IRateService
{
    Task<decimal> GetRate(string currency);
    Task<decimal> GetRate(string currency, DateTime date);
}

[Decorate]
public partial class RetryWrapper : IRateService
{
    private readonly IRateService _rateService;
    
    public RetryWrapper(IRateService rateService) => _rateService = rateService;
    
    [Template]
    public async Task<T> Retry<T>(Func<Task<T>> action)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                if (retryCount++ >= 3)
                    throw;
                Console.WriteLine($"Retry {nameof(action)} {retryCount} due to {e.Message}");
            }
        }
    }
}