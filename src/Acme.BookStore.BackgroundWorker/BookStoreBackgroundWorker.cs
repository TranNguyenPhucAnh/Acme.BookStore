using Acme.BookStore.Schedulers;
using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using NCrontab;

namespace Acme.BookStore.BackgroundWorker
{
    public class BookStoreBackgroundWorker: QuartzBackgroundWorkerBase
    {
        private readonly ISchedulerAppService _schedulerAppService;
        public BookStoreBackgroundWorker(
            ISchedulerAppService schedulerAppService)
        {
            _schedulerAppService = schedulerAppService;

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
            Logger.LogInformation("Executed MyLogWorker..!");
            var schedulers = await _schedulerAppService.GetAllAsync();
          
            var nextOccurences = schedulers.Items.Select(s => CrontabSchedule.Parse(s.CronExpression));
            
        }
    }
}
