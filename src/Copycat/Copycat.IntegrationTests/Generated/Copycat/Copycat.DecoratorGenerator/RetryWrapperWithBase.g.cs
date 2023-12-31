﻿// <auto-generated/>
using Serilog;

namespace Copycat.IntegrationTests;
public partial class RetryWrapperWithBase
{
    /// <see cref = "ResiliencyBase.Retry{T}(Func{Task{T}})"/>
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
                Logger.Warning("Retry {Action} {RetryCount} due to {Message}", nameof(GetRate), retryCount, e.Message);
            }
        }
    }

    /// <see cref = "ResiliencyBase.Retry{T}(Func{Task{T}})"/>
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
                Logger.Warning("Retry {Action} {RetryCount} due to {Message}", nameof(GetRate), retryCount, e.Message);
            }
        }
    }
}