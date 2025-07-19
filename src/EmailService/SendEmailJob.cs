using Quartz;

namespace EmailService;

[DisallowConcurrentExecution]
public class SendEmailJob(IEmailService emailService) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return emailService.SendPaymentReminders();
    }
}
