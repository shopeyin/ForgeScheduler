
using ForgeScheduler.Api.Dtos.Jobs;
using ForgeScheduler.Application.Abstractions;
using ForgeScheduler.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ForgeScheduler.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> _logger;
        private readonly IJobRepository _jobRepository;

        public JobsController(ILogger<JobsController> logger, IJobRepository jobRepository)
        {
            _logger = logger;
            _jobRepository = jobRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(CreateJobRequest request)
        {
            var job = new Job
            {
                Payload = request.Payload,
                ScheduledAt = request.ScheduledAt
            };

            var id = await _jobRepository.CreateJobAsync(job);

            return Ok(new
            {
                Id = id
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _jobRepository.GetAllJobsAsync();

            return Ok(jobs);
        }
    }
}
