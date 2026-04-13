using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class AccountController : Controller
{
  
    public IActionResult Login()
    {
        return View();
    }

    private readonly IConfiguration _config;

    public AccountController(IConfiguration config)
    {
        _config = config;

    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var users = new List<(string name, string pass)>
    {
        ("KaderNathAmer", "kader2008fqdsfdsqOjerz876832332P3O2KI3"),
        ("ZatoutHamza", "hamza2015JFEOIZkoeiiie7ç'_èè_-&"),
        ("GherbiMohamed", "Gherbi200sgf5re8_ç_àhfdjh52hg4"),
        ("admin", "1234dfghr-è455457g4")
    };

        var user = users.FirstOrDefault(u =>
            u.name == username && u.pass == password);

        if (user.name != null)
        {
            HttpContext.Session.SetString("user", user.name);
            return RedirectToAction("Messages", "Contact");
        }

        ViewBag.Error = "Wrong credentials";
        return View();
    }








}
