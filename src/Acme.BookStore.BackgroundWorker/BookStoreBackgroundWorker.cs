using Acme.BookStore.Schedulers;
using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using NCrontab;
using Acme.BookStore.Users;
using Volo.Abp.Emailing;
using Acme.BookStore.Books;
using Volo.Abp.Security.Claims;
using System.Security.Claims;
using Volo.Abp.TextTemplating;
using Volo.Abp.Emailing.Templates;

namespace Acme.BookStore.BackgroundWorker
{
    public class BookStoreBackgroundWorker : QuartzBackgroundWorkerBase
    {
        private readonly ISchedulerAppService _schedulerAppService;
        private readonly IEmailSender _emailSender;
        private readonly IUserAppService _userAppService;
        private readonly IBookAppService _bookAppService;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly ILogger<BookStoreBackgroundWorker> _logger;

        public BookStoreBackgroundWorker(
            ISchedulerAppService schedulerAppService,
            IEmailSender emailSender,
            IUserAppService userAppService,
            IBookAppService bookAppService,
            ICurrentPrincipalAccessor currentPrincipalAccessor,
            ITemplateRenderer templateRenderer,
            ILogger<BookStoreBackgroundWorker> logger
            )
        {
            _schedulerAppService = schedulerAppService;
            _emailSender = emailSender;
            _userAppService = userAppService;
            _bookAppService = bookAppService;
            _currentPrincipalAccessor = currentPrincipalAccessor;
            _templateRenderer = templateRenderer;
            _logger = logger;

            JobDetail = JobBuilder
                .Create<BookStoreBackgroundWorker>()
                .WithIdentity(nameof(BookStoreBackgroundWorker))
                .Build();
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(BookStoreBackgroundWorker))
                .WithCronSchedule("0 * * ? * *") // Every minute
                                                 //.StartNow()
                .Build();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            var utcNow = DateTime.UtcNow;
            Logger.LogInformation($"Executed Background Worker At {utcNow}.");

            var schedulers = await _schedulerAppService.GetAllAsync();

            var nextOccurences = schedulers.Items
                .SelectMany(s => CrontabSchedule.Parse(CronHelper.ConvertLocalCronToUtcCron(s.CronExpression, s.TimeZone))
                .GetNextOccurrences(utcNow.AddSeconds(-1), DateTime.UtcNow.AddMonths(1))
                .Select(occurrence => new SchedulerOccurenceDto(
                    s.RecipientEntityId,
                    s.RecipientEntity,
                    s.RecipientType,
                    s.WorkerOutputType,
                    occurrence
                    )))
                .OrderBy(o => o.NextOccurrence.Ticks);

            if (nextOccurences.Any())
            {
                var currentTriggers = nextOccurences
                    .Where(x => x.NextOccurrence.IsBetween(utcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(15))).ToList();
                if (currentTriggers.Count > 0)
                {
                    await SendMailsAsync(currentTriggers);
                }
            }
        }

        private async Task SendMailsAsync(List<SchedulerOccurenceDto> currentTriggers)
        {
            try
            {
                var admin = (await _userAppService.GetUsersByNormalizedRoleNameAsync("ADMIN")).FirstOrDefault();

                var newPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    [
                        new Claim (AbpClaimTypes.UserId, admin.Id.ToString()),
                        new Claim (AbpClaimTypes.Role, "admin"),
                    ]));

                using (_currentPrincipalAccessor.Change(newPrincipal))
                {
                    _logger.LogInformation($"Current user is set to {admin.UserName} with ID {admin.Id}.");

                    var file = await _bookAppService.ExportAsync();

                    var model = new
                    {
                        Username = admin.UserName,
                        LocalizedMessage = "Hi, please find the attachment in this email.",
                        Year = DateTime.Now.Year
                    };

                    var body = await _templateRenderer.RenderAsync(StandardEmailTemplates.Message, globalContext: new Dictionary<string, object>
                    {
                        { "model", model }
                    });

                    _logger.LogInformation(body);

                    foreach (var s in currentTriggers)
                    {
                        switch (s.RecipientType)
                        {
                            case RecipientTypeEnum.Individual:
                                var user = (await _userAppService.GetUsersAsync(new List<Guid>() { s.RecipientEntityId })).FirstOrDefault();

                                _logger.LogInformation($"Sending email attachment {file.FileDownloadName} to {user.Email} at {s.NextOccurrence} with body as {body}");

                                await _emailSender.QueueAsync(
                                    user.Email,
                                    "Scheduled Task Notification",
                                    body,
                                    true,
                                    new AdditionalEmailSendingArgs
                                    {
                                        Attachments = new List<EmailAttachment>
                                        {
                                            new EmailAttachment
                                            {
                                                Name = file.FileDownloadName,
                                                File = file.FileContents,
                                            }
                                        }
                                    }
                                );

                                _logger.LogInformation($"Email attachment {file.FileDownloadName} is sent to {user.Email} at {s.NextOccurrence}.");
                                break;

                            case RecipientTypeEnum.OrganizationBased:
                                var usersByOrg = await _userAppService.GetUsersInOrganizationUnitsAsync(new List<Guid>() { s.RecipientEntityId });
                                var firstEmail = usersByOrg.ElementAtOrDefault(0)?.Email;
                                usersByOrg.RemoveAt(0);

                                await _emailSender.QueueAsync(
                                    firstEmail,
                                    "Scheduled Task Notification",
                                    body,
                                    true,
                                    new AdditionalEmailSendingArgs
                                    {
                                        Attachments = new List<EmailAttachment>
                                        {
                                            new EmailAttachment
                                            {
                                                Name = file.FileDownloadName,
                                                File = file.FileContents,
                                            }
                                        },
                                        CC = [.. usersByOrg.Select(s => s.Email)]
                                    }
                                );
                                break;

                            case RecipientTypeEnum.RoleBased:
                                var usersByRole = await _userAppService.GetUsersByRolesAsync(s.RecipientEntityId);
                                var emailFirst = usersByRole.ElementAtOrDefault(0)?.Email;
                                usersByRole.RemoveAt(0);

                                await _emailSender.QueueAsync(
                                    emailFirst,
                                    "Scheduled Task Notification",
                                    body,
                                    true,
                                    new AdditionalEmailSendingArgs
                                    {
                                        Attachments = new List<EmailAttachment>
                                        {
                                            new EmailAttachment
                                            {
                                                Name = file.FileDownloadName,
                                                File = file.FileContents,
                                            }
                                        },
                                        CC = [.. usersByRole.Select(s => s.Email)]
                                    }
                                );
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error while sending emails.");
                throw;
            }
        }
    }
}