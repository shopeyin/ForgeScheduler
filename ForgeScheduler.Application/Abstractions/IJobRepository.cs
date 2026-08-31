using ForgeScheduler.Domain;


namespace ForgeScheduler.Application.Abstractions
{
    public interface IJobRepository
    {
        Task<int> CreateJobAsync(Job job);

        Task<IEnumerable<Job>> GetAllJobsAsync();

        Task<IEnumerable<Job>> GetDueJobsAsync();

        Task MarkProcessingAsync(int id);

        Task MarkCompletedAsync(int id);

        Task RetryJobAsync(int id, DateTime nextRetryAt);

        Task MarkPermanentlyFailedAsync(int id);
    }
}
