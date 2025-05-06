using CoinVista.Models;
using System.Text.Json;

namespace CoinVista.Services
{
    public class InvestmentService
    {
        private readonly string _filePath = "investments.json";
        private List<Investment> _data;
        private int _nextId;

        public InvestmentService()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<List<Investment>>(json) ?? new();
            }
            else
            {
                _data = new List<Investment>();
            }

            // Determine next ID
            _nextId = _data.Any() ? _data.Max(i => i.Id) + 1 : 1;
        }

        private void SaveChanges()
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public List<Investment> GetAll(string email) =>
            _data.Where(i => i.Email == email).ToList();

        public Investment? GetById(int id) =>
            _data.FirstOrDefault(i => i.Id == id);

        public void Add(Investment inv)
        {
            inv.Id = _nextId++;
            _data.Add(inv);
            SaveChanges();
        }

        public void Update(Investment inv)
        {
            var index = _data.FindIndex(i => i.Id == inv.Id);
            if (index != -1)
            {
                _data[index] = inv;
                SaveChanges();
            }
        }

        public void Delete(int id)
        {
            _data.RemoveAll(i => i.Id == id);
            SaveChanges();
        }
    }
}
