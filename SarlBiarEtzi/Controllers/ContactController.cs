using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SarlBiarEtzi.Models;
using SarlBiarEtzi.Services;

namespace SarlBiarEtzi.Controllers
{
    public class ContactController : Controller
    {
        private readonly string _connectionString;
        private readonly GroqService _groq;

        public ContactController(IConfiguration configuration, GroqService groq)
        {
            var url =
                Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
                ?? configuration.GetConnectionString("DefaultConnection");

            _connectionString = ParseDatabaseUrl(url);
            _groq = groq;
        }

        /* ================= CONTACT PAGE ================= */
        public IActionResult Contact()
    {
        return View();
    }

    /* ================= SAVE CONTACT FORM ================= */
    [HttpPost]
    public IActionResult Send(Contact model)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var cmd = new NpgsqlCommand(
            "INSERT INTO contacts (name, email, message) VALUES (@name, @email, @message)",
            conn
        );

        cmd.Parameters.AddWithValue("@name", model.Name);
        cmd.Parameters.AddWithValue("@email", model.Email);
        cmd.Parameters.AddWithValue("@message", model.Message);

        cmd.ExecuteNonQuery();

        ViewBag.Message = "تم الإرسال بنجاح ✅";
        return View("Contact");
    }

    /* ================= ADMIN MESSAGES PAGE ================= */
    public IActionResult Messages()
    {
        if (HttpContext.Session.GetString("user") == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var list = new List<Contact>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var cmd = new NpgsqlCommand(
            "SELECT id, name, email, message FROM contacts ORDER BY id DESC",
            conn
        );

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new Contact
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Message = reader.GetString(3)
            });
        }

        return View(list);
    }

    /* ================= DELETE CONTACT ================= */
    public IActionResult Delete(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var cmd = new NpgsqlCommand(
            "DELETE FROM contacts WHERE id = @id",
            conn
        );

        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return RedirectToAction("Messages");
    }

    /* ================= CHAT LOAD MESSAGES (FIXED) ================= */
    [HttpGet]
    public JsonResult GetRoomMessages()
    {
        var list = new List<object>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string sql = @"
        SELECT sender, message
        FROM chat_messages 
        ORDER BY created_at ASC
    ";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new
            {
                sender = reader.GetString(0),
                message = reader.GetString(1)
            });
        }

        return Json(list);
    }

        [HttpPost]
        public async Task<JsonResult> SendToBot([FromBody] string message)
        {
            var result = await _groq.SendMessage(message);
            return Json(new { reply = result });
        }



        private string ParseDatabaseUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("Missing DATABASE_URL");

            if (!url.StartsWith("postgresql://"))
                return url;

            var uri = new Uri(url);

            var userInfo = uri.UserInfo.Split(':');

            var user = userInfo[0];
            var password = userInfo[1];

            return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={user};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
        }



    }
}
