using ForgeScheduler.Data;
using ForgeScheduler.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ForgeScheduler.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> _logger;
        private readonly JobRepository _jobRepository;

        public JobsController(ILogger<JobsController> logger, JobRepository jobRepository)
        {
            _logger = logger;
            _jobRepository = jobRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(Job job)
        {
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
