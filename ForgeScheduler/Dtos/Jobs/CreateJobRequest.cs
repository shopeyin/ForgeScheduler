namespace ForgeScheduler.Api.Dtos.Jobs
{
    public class CreateJobRequest
    {
        public string Payload { get; set; } = "{}";
        public DateTime? ScheduledAt { get; set; }
    }
}
