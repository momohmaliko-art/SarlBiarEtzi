using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SarlBiarEtzi.Hubs
{
    public class NotificationHub : Hub
    {
        // 🔥 إرسال رسالة لكل المتصلين
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        // 🔥 عند دخول مستخدم
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveNotification",
                "👋 Welcome to Sarl Biar Etzi website");

            await base.OnConnectedAsync();
        }

        // 🔥 عند خروج مستخدم
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
