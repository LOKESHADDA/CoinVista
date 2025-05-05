using Microsoft.AspNetCore.Mvc;
using CoinVista.Services;
using CoinVista.Models;

namespace CoinVista.Controllers
{
    public class DashboardController : Controller
    {
        private readonly InvestmentService _invSvc;
        private readonly CoinGeckoService _coinSvc;
        private readonly CoinHistoryCacheService _cacheSvc;

        public DashboardController(
            InvestmentService invSvc,
            CoinGeckoService coinSvc,
            CoinHistoryCacheService cacheSvc)
        {
            _invSvc = invSvc;
            _coinSvc = coinSvc;
            _cacheSvc = cacheSvc;
        }

        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var investments = _invSvc.GetAll(email) ?? new List<Investment>();
            if (!investments.Any())
            {
                ViewBag.TotalInvested     = 0m;
                ViewBag.TotalCurrent      = 0m;
                ViewBag.ProfitLossPct     = 0m;
                ViewBag.TrendDates        = new List<string>();
                ViewBag.TrendPrices       = new List<decimal>();
                ViewBag.PLLabels          = new List<string>();
                ViewBag.PLData            = new List<decimal>();
                ViewBag.AllocationLabels  = new List<string>();
                ViewBag.AllocationData    = new List<decimal>();
                ViewBag.MultiLineDatasets = new List<object>();
                return View(investments);
            }

            // Group by CryptoId and sum quantity/amount
            var grouped = investments
                .GroupBy(i => i.CryptoId)
                .Select(g => new
                {
                    CryptoId = g.Key,
                    CryptoName = g.First().CryptoName ?? g.Key,
                    TotalAmount = g.Sum(x => x.AmountInvested),
                    TotalQuantity = g.Sum(x => x.Quantity),
                    Sample = g.First()
                })
                .ToList();

            var enriched = new List<(
                string CryptoId,
                string CryptoName,
                decimal TotalAmount,
                decimal TotalQuantity,
                CoinDetails Details,
                List<(DateTime Date, decimal Price)> History
            )>();

            foreach (var coin in grouped)
            {
                CoinDetails details;
                try
                {
                    details = await _coinSvc.GetCoinDetailsById(coin.CryptoId);
                }
                catch
                {
                    details = new CoinDetails
                    {
                        Id = coin.CryptoId,
                        Name = coin.CryptoName,
                        CurrentPrice = coin.Sample.PurchasePrice
                    };
                }

                var history = await _cacheSvc.GetOrFetchHistory(coin.CryptoId);

                enriched.Add((
                    coin.CryptoId,
                    coin.CryptoName,
                    coin.TotalAmount,
                    coin.TotalQuantity,
                    details,
                    history
                ));
            }

            // Total Invested & Current
            var totalInvested = enriched.Sum(x => x.TotalAmount);
            var totalCurrent = enriched.Sum(x => x.TotalQuantity * x.Details.CurrentPrice);
            ViewBag.TotalInvested = totalInvested;
            ViewBag.TotalCurrent = totalCurrent;
            ViewBag.ProfitLossPct = totalInvested == 0
                ? 0
                : (totalCurrent - totalInvested) / totalInvested * 100;

            // Date labels (MM/dd)
            var dates = enriched
                .SelectMany(x => x.History)
                .Select(h => h.Date.Date)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => d.ToString("MM/dd"))
                .ToList();
            ViewBag.TrendDates = dates;

            // Portfolio Trend Over Days (in %)
            var portfolioTrend = dates
                .Select(dateLabel =>
                    enriched.Sum(x =>
                    {
                        var match = x.History.FirstOrDefault(h => h.Date.ToString("MM/dd") == dateLabel);
                        return match != default ? x.TotalQuantity * match.Price : 0;
                    })
                ).ToList();

            var baseValue = portfolioTrend.FirstOrDefault();
            ViewBag.TrendPrices = portfolioTrend
                .Select(val => baseValue == 0 ? 0 : (val - baseValue) / baseValue * 100)
                .ToList();

            // Daily P/L % Change
            ViewBag.PLLabels = dates;
            ViewBag.PLData = portfolioTrend
                .Select((val, idx) =>
                {
                    if (idx == 0 || portfolioTrend[idx - 1] == 0) return 0;
                    return (val - portfolioTrend[idx - 1]) / portfolioTrend[idx - 1] * 100;
                })
                .ToList();

            // Allocation %
            var allocValues = enriched
                .Select(x => x.TotalQuantity * x.Details.CurrentPrice)
                .ToList();
            var total = allocValues.Sum();
            ViewBag.AllocationLabels = enriched.Select(x => x.CryptoName).ToList();
            ViewBag.AllocationData = allocValues
                .Select(val => total == 0 ? 0 : (val / total) * 100)
                .ToList();

            // Multi-line Chart per Coin % Change
            ViewBag.MultiLineDatasets = enriched
                .Select(x =>
                {
                    var baseP = x.History.FirstOrDefault().Price;
                    return new
                    {
                        label = x.CryptoName,
                        data = x.History.Select(h =>
                            baseP == 0 ? 0 : (h.Price - baseP) / baseP * 100
                        ).ToList(),
                        tension = 0.3m
                    };
                })
                .ToList();

            return View(investments);
        }
    }
}