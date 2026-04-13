using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SarlBiarEtzi.Hubs;
using System;
using System.Net;
using System.Net.Mail;

namespace SarlBiarEtzi.Controllers
{
    public class HomeController : Controller
    {
        private static string SavedEmail;
        private static string SavedOTP;

        private readonly IHubContext<NotificationHub> _hub;

        // ================= SMTP CONFIG =================
        private string smtpEmail = "sarlbiar.boot.support@gmail.com";
        private string smtpPassword = "effy zgun bsfw msri";

        public HomeController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        public IActionResult Reviews()
        {
            var reviews = new List<SarlBiarEtzi.Models.Review>();

            using (var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=sarlbiaretzi;Username=postgres;Password=0"))
            {
                conn.Open();

                string sql = "SELECT id, email, stars, comment, createdat FROM reviews ORDER BY id DESC";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new SarlBiarEtzi.Models.Review
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Email = reader["email"].ToString(),
                            Stars = Convert.ToInt32(reader["stars"]),
                            Comment = reader["comment"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["createdat"])
                        });
                    }
                }
            }

            return View(reviews);
        }


        // ================= OTP SEND =================
        [HttpPost]
        public IActionResult SendOTP(string email)
        {
            SavedEmail = email;
            SavedOTP = new Random().Next(100000, 999999).ToString();

            // ================= SEND EMAIL =================
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
                    smtpClient.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress(smtpEmail);
                    mail.To.Add(email);
                    mail.Subject = "OTP Verification Code";
                    mail.Body = $"Your OTP Code is: {SavedOTP}";

                    smtpClient.Send(mail);
                }

                _hub.Clients.All.SendAsync("ReceiveNotification",
                    $"📩 OTP sent to {email}");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = true });
        }

        // ================= OTP VERIFY =================
        [HttpPost]
        public IActionResult VerifyOTP(string email, string otp)
        {
            if (email == SavedEmail && otp == SavedOTP)
            {
                HttpContext.Session.SetString("user", email);

                _hub.Clients.All.SendAsync("ReceiveNotification",
                    $"✅ {email} logged in");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // ================= ADD REVIEW =================
        [HttpPost]
        public IActionResult AddReview(int stars, string comment)
        {
            var email = HttpContext.Session.GetString("user");

            if (email == null)
                return RedirectToAction("Reviews");

            using (var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=sarlbiaretzi;Username=postgres;Password=0"))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO reviews(email, stars, comment, createdat)
                    VALUES(@email, @stars, @comment, NOW())
                ";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@stars", stars);
                    cmd.Parameters.AddWithValue("@comment", comment);

                    cmd.ExecuteNonQuery();
                }
            }

            _hub.Clients.All.SendAsync("ReceiveNotification",
                $"⭐ New review from {email}");

            return RedirectToAction("Reviews");
        }
















        // ================= ERROR =================
        public IActionResult Error()
        {
            return View();
        }
    }
}
