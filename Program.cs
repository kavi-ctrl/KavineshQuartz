using Quartz;
using KavineshQuartz.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<JobStatusService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey("JsonCrudJob");

    options.AddJob<JsonCrudJob>(job =>
        job.WithIdentity(jobKey));

    options.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithIdentity("JsonCrudJob-trigger")
        .StartNow()
        .WithSimpleSchedule(schedule => schedule
            .WithIntervalInMinutes(1)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
