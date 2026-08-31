using Dapper;
using ForgeScheduler.Application.Abstractions;
using ForgeScheduler.Domain;
using System.Data;
namespace ForgeScheduler.Infrastructure.Persistence
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbConnection _db;

        public JobRepository(IDbConnection db)
        {
            _db = db;
        }
        public async Task<int> CreateJobAsync(Job job)
        {
            var sql = @"
                    INSERT INTO jobs (payload, scheduled_at)
                    VALUES (@Payload::jsonb, @ScheduledAt)
                    RETURNING id;
                ";

            return await _db.ExecuteScalarAsync<int>(sql, job);
        }
        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            var sql = "SELECT id, payload, status, attempts, created_at, scheduled_at, locked_at, locked_by FROM jobs";

            return await _db.QueryAsync<Job>(sql);
        }


        public async Task<IEnumerable<Job>> GetDueJobsAsync()
        {
            const string sql = """
                SELECT
                    id,
                    payload,
                    status,
                    attempts,
                    created_at,
                    scheduled_at,
                    locked_at,
                    locked_by
                FROM jobs
                WHERE status = 'pending'
                  AND scheduled_at <= NOW()
                  AND locked_at IS NULL
                ORDER BY scheduled_at
                LIMIT 10;
                """;

            return await _db.QueryAsync<Job>(sql);
        }

        public async Task MarkProcessingAsync(int id)
        {
            const string sql = """
            UPDATE jobs
            SET status = 'running'
            WHERE id = @Id;
            """;

            await _db.ExecuteAsync(sql, new { Id = id });
        }
        public async Task LockJobAsync(int id, string workerId)
        {
            const string sql = """
                UPDATE jobs
                SET
                    locked_at = NOW(),
                    locked_by = @WorkerId,
                    status = 'running'
                WHERE id = @Id;
                """;

            await _db.ExecuteAsync(sql, new
            {
                Id = id,
                WorkerId = workerId
            });
        }

        public async Task MarkCompletedAsync(int id)
        {
            const string sql = """
                    UPDATE jobs
                    SET
                        status = 'completed',
                        last_successfully_completed_at = NOW(),
                        locked_at = NULL,
                        locked_by = NULL
                    WHERE id = @Id;
                    """;

            await _db.ExecuteAsync(sql, new
            {
                Id = id
            });
        }


        public async Task RetryJobAsync(int id, DateTime nextAttempt)
        {
            const string sql = """
                UPDATE jobs
                SET
                    status = 'pending',
                    attempts = attempts + 1,
                    scheduled_at = @NextAttempt
                WHERE id = @Id;
                """;

            await _db.ExecuteAsync(sql, new
            {
                Id = id,
                NextAttempt = nextAttempt
            });
        }

        public async Task MarkPermanentlyFailedAsync(int id)
        {
            const string sql = """
                UPDATE jobs
                SET
                    status = 'failed',
                    attempts = attempts + 1
                WHERE id = @Id;
                """;

            await _db.ExecuteAsync(sql, new { Id = id });
        }

        

    }
}
