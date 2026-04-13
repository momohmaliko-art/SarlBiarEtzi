using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace SarlBiarEtzi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _conn =
            "Host=localhost;Port=5432;Database=sarlbiaretzi;Username=postgres;Password=0";

        public async Task JoinGeneral()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "GENERAL");
        }

        public async Task LeaveGeneral()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "GENERAL");
        }

        public async Task SendMessage(string sender, string message)
        {
            using var conn = new NpgsqlConnection(_conn);
            await conn.OpenAsync();

            var sql = @"
        INSERT INTO chat_messages(sender, message, created_at)
        VALUES(@sender, @message, NOW());
    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@sender", sender);
            cmd.Parameters.AddWithValue("@message", message);

            await cmd.ExecuteNonQueryAsync();

            await Clients.All.SendAsync("ReceiveMessage", sender, message);
        }

    }
}