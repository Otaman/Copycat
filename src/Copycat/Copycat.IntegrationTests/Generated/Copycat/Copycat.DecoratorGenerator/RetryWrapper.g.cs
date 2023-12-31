﻿// <auto-generated/>
namespace Copycat.IntegrationTests;
public partial class RetryWrapper
{
    /// <see cref = "RetryWrapper.Retry{T}(Func{Task{T}})"/>
    public async Task<decimal> GetRate(string currency)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                return await _rateService.GetRate(currency);
            }
            catch (Exception e)
            {
                if (retryCount++ >= 3)
                    throw;
                Console.WriteLine($"Retry {nameof(GetRate)} {retryCount} due to {e.Message}");
            }
        }
    }

    /// <see cref = "RetryWrapper.Retry{T}(Func{Task{T}})"/>
    public async Task<decimal> GetRate(string currency, DateTime date)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                return await _rateService.GetRate(currency, date);
            }
            catch (Exception e)
            {
                if (retryCount++ >= 3)
                    throw;
                Console.WriteLine($"Retry {nameof(GetRate)} {retryCount} due to {e.Message}");
            }
        }
    }
}