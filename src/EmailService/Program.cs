using EmailService;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

    JobKey jobKey = new(nameof(SendEmailJob));
    quartzConfiguration.AddJob<SendEmailJob>(jobKey)
        .AddTrigger(trigger =>
            trigger.ForJob(jobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInSeconds(5)
                        .RepeatForever()));
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

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
