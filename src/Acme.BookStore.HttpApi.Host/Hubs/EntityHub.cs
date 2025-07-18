using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace Acme.BookStore.Hubs
{
    public class EntityHub : AbpHub
    {
        //push message to remind the listener to call GET min date API
        public async Task SendBookListReload(string bookEvent)
        {
            await Clients.All.SendAsync("BookEventThatNeedReloadList", bookEvent);
        }

        public async Task SendAuthorListReload(string authorEvent)
        {
            await Clients.All.SendAsync("AuthorEventThatNeedReloadList", authorEvent);
        }
    }
}