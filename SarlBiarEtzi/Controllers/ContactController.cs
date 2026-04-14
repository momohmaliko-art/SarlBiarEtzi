using Microsoft.AspNetCore.Mvc;
using SarlBiarEtzi.Models;
using SarlBiarEtzi.Services;

namespace SarlBiarEtzi.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GroqService _groq;

        public ContactController(AppDbContext context, GroqService groq)
        {
            _context = context;
            _groq = groq;
        }

        // ================= CONTACT PAGE =================
        public IActionResult Contact()
        {
            return View();
        }

        // ================= SAVE CONTACT =================
        [HttpPost]
        public IActionResult Send(Contact model)
        {
            try
            {
                if (model == null)
                {
                    ViewBag.Message = "بيانات فارغة ❌";
                    return View("Contact");
                }

                _context.Contacts.Add(model);
                _context.SaveChanges();

                ViewBag.Message = "تم الإرسال بنجاح ✅";
            }
            catch (Exception ex)
            {
                Console.WriteLine("CONTACT ERROR: " + ex.Message);
                ViewBag.Message = "خطأ في قاعدة البيانات ❌";
            }

            return View("Contact");
        }

        // ================= MESSAGES (ADMIN) =================
        public IActionResult Messages()
        {
            if (HttpContext.Session.GetString("user") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var list = _context.Contacts
                    .OrderByDescending(x => x.Id)
                    .ToList();

                return View(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MESSAGES ERROR: " + ex.Message);
                return View(new List<Contact>());
            }
        }

        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            try
            {
                var item = _context.Contacts.Find(id);

                if (item != null)
                {
                    _context.Contacts.Remove(item);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DELETE ERROR: " + ex.Message);
            }

            return RedirectToAction("Messages");
        }

        // ================= CHAT MESSAGES =================
        [HttpGet]
        public JsonResult GetRoomMessages()
        {
            try
            {
                var list = _context.ChatMessages
                    .OrderBy(x => x.Created_At)
                    .Select(x => new
                    {
                        sender = x.Sender,
                        message = x.Message
                    })
                    .ToList();

                return Json(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CHAT ERROR: " + ex.Message);
                return Json(new List<object>());
            }
        }

        // ================= AI BOT =================
        [HttpPost]
        public async Task<JsonResult> SendToBot([FromBody] string message)
        {
            try
            {
                var result = await _groq.SendMessage(message);
                return Json(new { reply = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine("BOT ERROR: " + ex.Message);
                return Json(new { reply = "خطأ في البوت" });
            }
        }
    }
}
