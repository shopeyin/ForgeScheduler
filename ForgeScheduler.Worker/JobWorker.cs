using ForgeScheduler.Application.Abstractions;
using ForgeScheduler.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForgeScheduler.Worker;

public class JobWorker : BackgroundService
{
    private const int MaxAttempts = 3;
    private const int BaseRetryDelaySeconds = 10;
    private const int MaxJitterSeconds = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobWorker> _logger;

    public JobWorker( IServiceScopeFactory scopeFactory,ILogger<JobWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IJobRepository>();

                var dispatcher =
                    scope.ServiceProvider
                        .GetRequiredService<JobDispatcher>();

                var jobs =
                    await repository.GetDueJobsAsync();

                foreach (var job in jobs)
                {
                    try
                    {
                        await repository.MarkProcessingAsync(job.Id);

                        _logger.LogInformation( "Executing job {JobId}",job.Id);

                        await dispatcher.DispatchAsync(job, stoppingToken);

                        await repository.MarkCompletedAsync(job.Id);

                        _logger.LogInformation( "Job {JobId} completed",job.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Job {JobId} failed",job.Id);

                        var nextAttemptNumber = job.Attempts + 1;

                        if (nextAttemptNumber >= MaxAttempts)
                        {
                            await repository.MarkPermanentlyFailedAsync(job.Id);

                            continue;
                        }

                        var exponentialDelay =
                            BaseRetryDelaySeconds *
                            Math.Pow(2, job.Attempts);

                        var jitter =
                            Random.Shared.Next(
                                0,
                                MaxJitterSeconds + 1);

                        var nextRetryAt =
                            DateTime.UtcNow.AddSeconds(
                                exponentialDelay + jitter);

                        await repository.RetryJobAsync(
                            job.Id,
                            nextRetryAt);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while polling jobs");
            }

            await Task.Delay( TimeSpan.FromSeconds(5),stoppingToken);
        }
    }
}