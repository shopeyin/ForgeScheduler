using ForgeScheduler.Application.Abstractions;
using ForgeScheduler.Domain;
using System.Text.Json;


namespace ForgeScheduler.Application.Jobs
{
    public class JobDispatcher
    {
        private readonly IEnumerable<IJobHandler> _handlers;

        public JobDispatcher(IEnumerable<IJobHandler> handlers)
        {
            _handlers = handlers;
        }


        public async Task DispatchAsync(Job job, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<JobPayload>(job.Payload);

            if (payload == null)
            {
                throw new InvalidOperationException("Invalid job payload.");
            }

            var handler = _handlers.FirstOrDefault(x => x.JobType == payload.Type);

            if (handler == null)
            {
                throw new InvalidOperationException($"No handler found for job type '{payload.Type}'.");
            }

            await handler.HandleAsync(job,cancellationToken);
        }
    }
}
