namespace ForgeScheduler.Models
{
    public class Job
    {
        public int Id { get; set; }

        public string Payload { get; set; } = "{}";

        public string Status { get; set; } = "pending";

        public int Attempts { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? LockedAt { get; set; }

        public string? LockedBy { get; set; }
    }
}
