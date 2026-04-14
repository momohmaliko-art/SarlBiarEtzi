using Microsoft.AspNetCore.SignalR;
using SarlBiarEtzi.Models;
using Microsoft.EntityFrameworkCore;

namespace SarlBiarEtzi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        // ================= JOIN GROUP =================
        public async Task JoinGeneral()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "GENERAL");
        }

        // ================= LEAVE GROUP =================
        public async Task LeaveGeneral()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "GENERAL");
        }

        // ================= SEND MESSAGE (EF) =================
        public async Task SendMessage(string sender, string message)
        {
            try
            {
                var chat = new ChatMessage
                {
                    Sender = sender ?? "unknown",
                    Message = message ?? "",
                    Created_At = DateTime.UtcNow
                };

                _db.ChatMessages.Add(chat);
                await _db.SaveChangesAsync();

                await Clients.All.SendAsync("ReceiveMessage", sender, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CHAT HUB ERROR: " + ex.Message);
            }
        }
    }
}
