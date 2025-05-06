using Microsoft.AspNetCore.Mvc;
namespace CoinVista.Controllers;
public class AboutController : Controller {
    public IActionResult Index() => View();
}