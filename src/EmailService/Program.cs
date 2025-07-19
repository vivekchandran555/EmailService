using EmailService;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEmailService, EmailService.EmailService>();

builder.Services.AddQuartz(quartzConfiguration =>
{
    quartzConfiguration.UsePersistentStore(persistentStoreoptions =>
    {
        persistentStoreoptions.UseProperties = true;
        persistentStoreoptions.UseClustering();
        persistentStoreoptions.UseSqlServer(builder.Configuration.GetConnectionString("Database")!);
        persistentStoreoptions.UseSystemTextJsonSerializer();
    });

    quartzConfiguration.SetProperty("quartz.scheduler.instanceId", "AUTO");
});

builder.Services.AddQuartzHostedService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/create-schedule", async (ISchedulerFactory schedulerFactory) =>
{
    var scheduler = await schedulerFactory.GetScheduler();

    var jobKey = new JobKey($"send-email", "email");

    if (await scheduler.CheckExists(jobKey))
    {
        return Results.Conflict("Job already scheduled.");
    }

    var jobDetail = JobBuilder.Create<SendEmailJob>()
        .WithIdentity(jobKey)
        .Build();

    var trigger = TriggerBuilder.Create()
        .WithIdentity($"trigger-send-email", "email")
        .ForJob(jobDetail)
        .WithCronSchedule("0 * * ? * *")
        .Build();

    await scheduler.ScheduleJob(jobDetail, trigger);

    return Results.Ok("Schedule created successfully.");
})
.WithName("CreateSchedule")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
