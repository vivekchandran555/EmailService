using Microsoft.Data.SqlClient;

namespace EmailService;

public interface IEmailService
{
    Task SendPaymentReminders();
}

public class EmailService(ILogger<EmailService> logger, IConfiguration configuration) : IEmailService
{
    public async Task SendPaymentReminders()
    {
        logger.LogInformation("Sending payment reminders...");

        // Insert into Log table using ADO.NET
        var connectionString = configuration.GetConnectionString("Database");
        var logMessage = "Payment reminder sent successfully";
        var utcTime = DateTime.UtcNow;

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand("INSERT INTO Log (Message, UtcTime) VALUES (@Message, @UtcTime)", connection);
        command.Parameters.AddWithValue("@Message", logMessage);
        command.Parameters.AddWithValue("@UtcTime", utcTime);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        // Simulate sending payment reminders
        await Task.Delay(1000);
        logger.LogInformation("Payment reminders sent successfully.");
    }
}
