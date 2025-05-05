using Microsoft.AspNetCore.Mvc;
namespace CoinVista.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        // If user is already logged in, redirect to Dashboard
        if (HttpContext.Session.GetString("Email") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }
}