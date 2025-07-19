namespace EmailService;

public interface IEmailService
{
    Task SendPaymentReminders();
}

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public async Task SendPaymentReminders()
    {
        logger.LogInformation("Sending payment reminders...");
        await Task.Delay(1000);
        logger.LogInformation("Payment reminders sent successfully.");
    }
}
