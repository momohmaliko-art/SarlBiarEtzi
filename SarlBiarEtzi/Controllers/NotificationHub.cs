using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SarlBiarEtzi.Hubs
{
    public class NotificationHub : Hub
    {
        // ================= SEND TO ALL =================
        public async Task SendNotification(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        // ================= ON CONNECT =================
        public override async Task OnConnectedAsync()
        {
            try
            {
                var connectionId = Context.ConnectionId;

                Console.WriteLine($"User connected: {connectionId}");

                await Clients.Caller.SendAsync(
                    "ReceiveNotification",
                    "👋 Welcome to Sarl Biar Etzi website"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("SignalR Connect Error: " + ex.Message);
            }

            await base.OnConnectedAsync();
        }

        // ================= ON DISCONNECT =================
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                Console.WriteLine($"User disconnected: {connectionId}");

                if (exception != null)
                {
                    Console.WriteLine("Disconnect error: " + exception.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SignalR Disconnect Error: " + ex.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
