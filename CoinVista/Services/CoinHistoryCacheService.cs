using System.Text.Json;
using CoinVista.Models;

namespace CoinVista.Services
{
    public class CoinHistoryCacheService
    {
        private readonly CoinGeckoService _coinGecko;
        private readonly string _filePath = "wwwroot/data/coin_history.json";
        private Dictionary<string, List<(DateTime, decimal)>> _cache;

        public CoinHistoryCacheService(CoinGeckoService coinGecko)
        {
            _coinGecko = coinGecko;
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "{}");
            LoadCache();
        }

        private void LoadCache()
        {
            var json = File.ReadAllText(_filePath);
            var parsed = JsonSerializer.Deserialize<Dictionary<string, List<HistoryDTO>>>(json)
                         ?? new();

            _cache = parsed.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(x => (x.Date, x.Price)).ToList()
            );
        }

        private void SaveCache()
        {
            var json = JsonSerializer.Serialize(
                _cache.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(x => new HistoryDTO { Date = x.Item1, Price = x.Item2 }).ToList()
                ),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public async Task<List<(DateTime, decimal)>> GetOrFetchHistory(string coinId)
        {
            if (_cache.ContainsKey(coinId)) return _cache[coinId];

            try
            {
                var history = await _coinGecko.GetHistoricalPrices(coinId, 7);
                _cache[coinId] = history;
                SaveCache();
                return history;
            }
            catch
            {
                // fallback to dummy flat history
                var fallback = Enumerable.Range(0, 7)
                    .Select(i => (DateTime.Today.AddDays(-6 + i), 1m))
                    .ToList();
                return fallback;
            }
        }

        private class HistoryDTO
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }
    }
}
