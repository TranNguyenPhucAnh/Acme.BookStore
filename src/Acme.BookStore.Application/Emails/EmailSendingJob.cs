using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;

namespace Acme.BookStore.Emails
{
    public class EmailSendingJob(IEmailSender emailSender) : AsyncBackgroundJob<EmailSendingArgs>, ITransientDependency
    {
        private readonly IEmailSender _emailSender = emailSender;

        public override async Task ExecuteAsync(EmailSendingArgs args)
        {
            try
            {
                await _emailSender.SendAsync(
                    args.To,
                    args.Subject,
                    args.Body
                );
            }
            catch (Exception ex)
            {
                // Log lỗi tại đây
                Logger.LogError(ex, "Failed to send email to {To}", args.To);
                throw; // Ném lại ngoại lệ để job được đánh dấu là thất bại
            }
        }
    }
}