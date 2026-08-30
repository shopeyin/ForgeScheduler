using ForgeScheduler.Data;
using ForgeScheduler.Models;
using System.Text.Json;

namespace ForgeScheduler.BackgroundServices
{
    public class JobWorker : BackgroundService
    {
        private const int MaxAttempts = 3;
        private const int BaseRetryDelaySeconds = 10;
        private const int MaxJitterSeconds = 5;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<JobWorker> _logger;

        public JobWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<JobWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Service running}");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var repository =
                        scope.ServiceProvider.GetRequiredService<JobRepository>();

                    var jobs = await repository.GetDueJobsAsync();

                    foreach (var job in jobs)
                    {
                        try
                        {
                            if (job.Id == 2) throw new Exception("Something went wrong.");
                            _logger.LogInformation("Processing job {JobId}", job.Id);

                            await repository.LockJobAsync(job.Id, Environment.MachineName);

                            await ExecuteJobAsync( job,stoppingToken);

                            await repository.MarkCompletedAsync(job.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Job {JobId} failed",
                                job.Id);

                            var nextAttemptNumber = job.Attempts + 1;

                            if (nextAttemptNumber >= MaxAttempts)
                            {
                                await repository.MarkPermanentlyFailedAsync(job.Id);

                                _logger.LogError("Job {JobId} permanently failed after {Attempts} attempts",job.Id,nextAttemptNumber);
                            }
                            else
                            {
                                var exponentialDelaySeconds =
                                    BaseRetryDelaySeconds * Math.Pow(2, job.Attempts);

                                var jitterSeconds =
                                    Random.Shared.Next(0, MaxJitterSeconds + 1);

                                var totalDelaySeconds =
                                    exponentialDelaySeconds + jitterSeconds;

                                var nextRetryAt =
                                    DateTime.UtcNow.AddSeconds(totalDelaySeconds);

                                await repository.RetryJobAsync(
                                    job.Id,
                                    nextRetryAt);

                                _logger.LogInformation("Job {JobId} will retry in {DelaySeconds} seconds at {RetryAt}", job.Id,
                                    totalDelaySeconds,
                                    nextRetryAt);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while polling jobs");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }


        private async Task ExecuteJobAsync(Job job,CancellationToken cancellationToken)
        {
            var payload =  JsonSerializer.Deserialize<JobPayload>(job.Payload);

            if (payload == null)
            {
                throw new InvalidOperationException("Job payload is invalid.");
            }

            switch (payload.Type)
            {
                case "send-email":
                    _logger.LogInformation("Running send email job");
                    break;

                case "archive-contractors":

                    _logger.LogInformation("Running archive contractors job");

                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown job type: {payload.Type}");
            }

            await Task.CompletedTask;
        }
    }
}
