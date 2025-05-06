using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoinVista.Models;

namespace CoinVista.Services
{
    public class CoinGeckoService
    {
        private readonly HttpClient _http;
        private static List<CoinOption> _cachedCoins = new();
        private static DateTime _lastFetchTime = DateTime.MinValue;

        public CoinGeckoService(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.UserAgent.Clear();
            _http.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("CoinVistaApp", "1.0"));
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private class MarketCoin
        {
            public string id { get; set; } = "";
            public string symbol { get; set; } = "";
            public string name { get; set; } = "";
        }

        public async Task<List<CoinOption>> GetCoinOptions()
        {
            // Use cache if within 15 minutes
            if (_cachedCoins.Any() && DateTime.Now - _lastFetchTime < TimeSpan.FromMinutes(15))
                return _cachedCoins;

            var url = "https://api.coingecko.com/api/v3/coins/markets"
                    + "?vs_currency=usd"
                    + "&order=market_cap_desc"
                    + "&per_page=250"
                    + "&page=1"
                    + "&sparkline=false";

            try
            {
                var resp = await _http.GetAsync(url);

                if (resp.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine("Rate limit hit (429). Returning cached coins.");
                    return _cachedCoins;
                }

                if (resp.StatusCode == HttpStatusCode.Forbidden)
                    throw new Exception("CoinGecko denied the market request (403).");

                resp.EnsureSuccessStatusCode();

                var markets = await resp.Content.ReadFromJsonAsync<List<MarketCoin>>();
                _cachedCoins = markets!
                    .Select(c => new CoinOption
                    {
                        Id = c.id,
                        TrueName = $"{c.name} ({c.symbol.ToUpper()})"
                    })
                    .ToList();

                _lastFetchTime = DateTime.Now;
                return _cachedCoins;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CoinGecko error: {ex.Message}");
                return _cachedCoins;
            }
        }

        public async Task<CoinDetails> GetCoinDetailsById(string id)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/{id}";
            var resp = await _http.GetAsync(url);

            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
                throw new Exception($"Rate limit hit while fetching details for '{id}'.");

            if (resp.StatusCode == HttpStatusCode.Forbidden)
                throw new Exception($"CoinGecko denied details for '{id}'.");

            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            return new CoinDetails
            {
                Id = root.GetProperty("id").GetString()!,
                Name = root.GetProperty("name").GetString()!,
                CurrentPrice = root
                  .GetProperty("market_data")
                  .GetProperty("current_price")
                  .GetProperty("usd")
                  .GetDecimal()
            };
        }

        public async Task<List<(DateTime date, decimal price)>> GetHistoricalPrices(string id, int days)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/{id}/market_chart"
                    + $"?vs_currency=usd&days={days}&interval=daily";

            var resp = await _http.GetAsync(url);

            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
                throw new Exception($"Rate limit hit while fetching history for '{id}'.");

            if (resp.StatusCode == HttpStatusCode.Forbidden)
                throw new Exception($"CoinGecko denied history for '{id}'.");

            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var pricesArr = doc.RootElement.GetProperty("prices").EnumerateArray();

            return pricesArr
              .Select(el =>
              {
                  var ms = el[0].GetDouble();
                  var dt = DateTimeOffset
                    .FromUnixTimeMilliseconds((long)ms)
                    .DateTime
                    .Date;
                  var px = el[1].GetDecimal();
                  return (date: dt, price: px);
              })
              .ToList();
        }
    }
}
