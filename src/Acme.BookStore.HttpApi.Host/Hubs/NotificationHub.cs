using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace Acme.BookStore.Hubs
{
    public class NotificationHub : AbpHub
    {
        //push message to remind the listener to call GET notification API 
        public async Task SendNotificationListReload(string message)
        {
            await Clients.All.SendAsync("NotificationListReload", message);
        }
    }
}
