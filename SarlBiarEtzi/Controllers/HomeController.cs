using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SarlBiarEtzi.Hubs;
using SarlBiarEtzi.Models;
using System.Net;
using System.Net.Mail;

namespace SarlBiarEtzi.Controllers
{
    public class HomeController : Controller
    {
        private static string SavedEmail;
        private static string SavedOTP;

        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        private string smtpEmail = "sarlbiar.boot.support@gmail.com";
        private string smtpPassword = "effy zgun bsfw msri";

        public HomeController(AppDbContext db, IHubContext<NotificationHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ================= HOME =================
        public IActionResult Index() => View();
        public IActionResult Privacy() => View();

        // ================= REVIEWS (EF) =================
        public IActionResult Reviews()
        {
            var reviews = _db.Reviews
                .OrderByDescending(r => r.Id)
                .ToList();

            return View(reviews);
        }

        // ================= SEND OTP =================
        [HttpPost]
        public IActionResult SendOTP(string email)
        {
            SavedEmail = email;
            SavedOTP = new Random().Next(100000, 999999).ToString();

            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(smtpEmail, smtpPassword),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(smtpEmail),
                    Subject = "OTP Verification Code",
                    Body = $"Your OTP Code is: {SavedOTP}"
                };

                mail.To.Add(email);

                smtpClient.Send(mail);

                _hub.Clients.All.SendAsync("ReceiveNotification",
                    $"📩 OTP sent to {email}");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = true });
        }

        // ================= VERIFY OTP =================
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

        // ================= ADD REVIEW (EF) =================
        [HttpPost]
        public IActionResult AddReview(int stars, string comment)
        {
            var email = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Reviews");

            var review = new Review
            {
                Email = email,
                Stars = stars,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reviews.Add(review);
            _db.SaveChanges();

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
