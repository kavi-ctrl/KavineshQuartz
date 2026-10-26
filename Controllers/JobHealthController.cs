using Microsoft.AspNetCore.Mvc;
using KavineshQuartz.Jobs;


namespace KavineshQuartz.Controllers
{

    [ApiController]
    [Route("health/job")]
    public class JobHealthController : ControllerBase
    {
        private readonly JobStatusService _jobStatus;

        public JobHealthController(JobStatusService jobStatus)
        {
            _jobStatus = jobStatus;
        }

        [HttpGet]
        public IActionResult GetJobHealth()
        {
            var status = _jobStatus.GetStatus();

            bool isHealthy = status.LastRun.HasValue &&
                DateTime.UtcNow - status.LastRun.Value <= TimeSpan.FromMinutes(5);

            var response = new
            {
                status = isHealthy ? "Running" : "Unhealthy",
                lastRun = status.LastRun?.ToString("O"),
                nextRun = status.NextRun?.ToString("O"),
                jobName = "JsonCrudJob"
            };

            return isHealthy ? Ok(response) : StatusCode(500, response);
        }
    }
}