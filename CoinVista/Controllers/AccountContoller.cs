using Microsoft.AspNetCore.Mvc;
using CoinVista.Models;
using CoinVista.Services;

namespace CoinVista.Controllers;

public class AccountController : Controller
{
    private readonly UserService _users;

    public AccountController(UserService u) => _users = u;

    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(RegisterModel m)
    {
        if (m.Password != m.ConfirmPassword)
            ModelState.AddModelError("", "Password and confirmation do not match.");

        if (!ModelState.IsValid)
            return View(m);

        var user = new User
        {
            Email = m.Email,
            FullName = m.FullName
        };

        try
        {
            _users.Register(user, m.Password);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(m);
        }

        return RedirectToAction("Login");
    }

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LoginModel m)
    {
        var user = _users.Authenticate(m.Email, m.Password);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(m);
        }

        HttpContext.Session.SetString("UserEmail", user.Email);
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}