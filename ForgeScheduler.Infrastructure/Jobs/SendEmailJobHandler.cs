using ForgeScheduler.Application.Abstractions;
using ForgeScheduler.Domain;


namespace ForgeScheduler.Infrastructure.Jobs
{
    public class SendEmailJobHandler : IJobHandler
    {
        public string JobType => "send-email";

        public Task HandleAsync(Job job, CancellationToken cancellationToken)
        {
            // Implement email sending logic here
            Console.WriteLine($"Executing send-email job {job.Id}");
            return Task.CompletedTask;
        }
    }
}
