using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace SarlBiarEtzi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _conn;

        public ChatHub(IConfiguration config)
        {
            _conn =
                Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
                ?? config.GetConnectionString("DefaultConnection");
        }

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
            cmd.Parameters.AddWithValue("@sender", sender ?? "unknown");
            cmd.Parameters.AddWithValue("@message", message ?? "");

            await cmd.ExecuteNonQueryAsync();

            await Clients.All.SendAsync("ReceiveMessage", sender, message);
        }
    }
}
