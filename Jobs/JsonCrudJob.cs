using System.Text.Json;
using Quartz;
using KavineshQuartz.Models;

namespace KavineshQuartz.Jobs
{
    [DisallowConcurrentExecution]
    public class JsonCrudJob : IJob
    {
        private readonly ILogger<JsonCrudJob> _logger;
        private readonly JobStatusService _jobStatus;

        private readonly string _filePath;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public JsonCrudJob(
            ILogger<JsonCrudJob> logger,
            JobStatusService jobStatus,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _jobStatus = jobStatus;
            _filePath = Path.Combine(
                environment.ContentRootPath,
                "data.json");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("JsonCrudJob started at {Time}", DateTime.UtcNow);

            try
            {
                List<DataItem> items = new();

                if (File.Exists(_filePath))
                {
                    string json = await File.ReadAllTextAsync(
                        _filePath,
                        context.CancellationToken);

                    items = JsonSerializer.Deserialize<List<DataItem>>(json)
                            ?? new List<DataItem>();
                }

                // create
                const string targetName = "Sample Item";

                var existingItem = items.FirstOrDefault(
                    x => string.Equals(
                        x.Name,
                        targetName,
                        StringComparison.OrdinalIgnoreCase));

                if (existingItem == null)
                {
                    int nextId = items.Count == 0
                        ? 1
                        : items.Max(x => x.Id) + 1;

                    items.Add(new DataItem
                    {
                        Id = nextId,
                        Name = targetName,
                        CreatedAt = DateTime.UtcNow
                    });

                    _logger.LogInformation("Added new item: {Name}", targetName);
                }
                else
                {
                    // update
                    existingItem.CreatedAt = DateTime.UtcNow;

                    _logger.LogInformation("Updated existing item: {Name}", targetName);
                }

                // delete
                DateTime cutoff = DateTime.UtcNow.AddDays(-1);

                int deletedCount = items.RemoveAll(
                    x => x.CreatedAt < cutoff);

                if (deletedCount > 0)
                {
                    _logger.LogInformation(
                        "Deleted {Count} old items", deletedCount);
                }

                
                string updatedJson = JsonSerializer.Serialize(
                    items, JsonOptions);

                await File.WriteAllTextAsync(
                    _filePath,
                    updatedJson,
                    context.CancellationToken);

                
                _jobStatus.MarkSuccess();

                _logger.LogInformation(
                    "JsonCrudJob completed successfully at {Time}",
                    DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JsonCrudJob failed");

                throw;
            }
        }
    }
}