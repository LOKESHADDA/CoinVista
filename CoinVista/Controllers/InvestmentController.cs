using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CoinVista.Models;
using CoinVista.Services;

namespace CoinVista.Controllers
{
    public class InvestmentController : Controller
    {
        private readonly InvestmentService _investmentService;
        private readonly CoinGeckoService _coinService;
        private readonly UserService _userService;

        public InvestmentController(
            InvestmentService investmentService,
            CoinGeckoService coinService,
            UserService userService)
        {
            _investmentService = investmentService;
            _coinService = coinService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login", "Account");
            var list = _investmentService.GetAll(email);
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            var coins = await _coinService.GetCoinOptions();
            ViewBag.CoinOptions = new SelectList(coins, "Id", "TrueName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Investment inv)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login", "Account");

            var coin = await _coinService.GetCoinDetailsById(inv.CryptoId);
            inv.CryptoName = coin.Name;
            inv.PurchasePrice = coin.CurrentPrice;
            inv.Quantity = inv.AmountInvested / coin.CurrentPrice;
            inv.InvestmentDate = DateTime.Now;
            inv.Email = email;

            _investmentService.Add(inv);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login", "Account");

            var inv = _investmentService.GetById(id);
            if (inv == null || inv.Email != email) return NotFound();

            var coins = await _coinService.GetCoinOptions();
            ViewBag.CoinOptions = new SelectList(coins, "Id", "TrueName", inv.CryptoId);
            return View(inv);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Investment inv)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login", "Account");

            var existing = _investmentService.GetById(inv.Id);
            if (existing == null || existing.Email != email) return NotFound();

            var coin = await _coinService.GetCoinDetailsById(inv.CryptoId);

            // Update necessary fields
            existing.CryptoId = inv.CryptoId;
            existing.CryptoName = coin.Name;
            existing.AmountInvested = inv.AmountInvested;
            existing.PurchasePrice = coin.CurrentPrice;
            existing.Quantity = inv.AmountInvested / coin.CurrentPrice;
            //  Retain the original investment date
            // existing.InvestmentDate stays untouched
            existing.Email = email;

            _investmentService.Update(existing);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login", "Account");

            var inv = _investmentService.GetById(id);
            if (inv == null || inv.Email != email) return NotFound();
            return View(inv);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _investmentService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
