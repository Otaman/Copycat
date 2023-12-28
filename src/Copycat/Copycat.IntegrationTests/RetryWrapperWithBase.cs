using Serilog;

namespace Copycat.IntegrationTests;

public interface ISomeService
{
    Task<decimal> GetRate(string currency);
    Task<decimal> GetRate(string currency, DateTime date);
}

public abstract class ResiliencyBase
{
    protected readonly ILogger Logger;
    
    protected ResiliencyBase(ILogger logger) => 
        Logger = logger.ForContext(GetType());
    
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
                
                Logger.Warning("Retry {Action} {RetryCount} due to {Message}", nameof(action), retryCount, e.Message);
            }
        }
    }
}

[Decorate]
public partial class RetryWrapperWithBase : ResiliencyBase, ISomeService
{
    private readonly ISomeService _rateService;
    
    public RetryWrapperWithBase(ISomeService rateService, ILogger logger) : base(logger) => 
        _rateService = rateService;
}