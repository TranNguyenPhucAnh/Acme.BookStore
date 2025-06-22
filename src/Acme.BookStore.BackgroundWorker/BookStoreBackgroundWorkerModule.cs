using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Modularity;

namespace Acme.BookStore.BackgroundWorker
{
    [DependsOn(
        //...other dependencies
        typeof(AbpBackgroundWorkersQuartzModule)
        //typeof(BookStoreApplicationContractsModule)
        )]
    public class BookStoreBackgroundWorkerModule : AbpModule
    {
    }
}
