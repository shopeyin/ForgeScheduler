using Dapper;
using ForgeScheduler.Models;
using System.Data;
namespace ForgeScheduler.Data
{
    public class JobRepository
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
            locked_at = NULL,
            locked_by = NULL
        WHERE id = @Id;
        """;

            await _db.ExecuteAsync(sql, new
            {
                Id = id
            });
        }
    }
}
