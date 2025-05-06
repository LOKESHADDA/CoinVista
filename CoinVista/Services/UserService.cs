using System.Text.Json;
using CoinVista.Models;
using Microsoft.AspNetCore.Identity;

namespace CoinVista.Services
{
    public class UserService
    {
        private readonly string _filePath = "users.json";
        private readonly PasswordHasher<User> _hasher = new();

        private List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public void Register(User u, string plainPassword)
        {
            var users = LoadUsers();
            if (users.Any(user => user.Email == u.Email))
                throw new Exception("User with this email already exists.");

            u.PasswordHash = _hasher.HashPassword(u, plainPassword);
            users.Add(u);
            SaveUsers(users);
        }

        public User? Authenticate(string email, string plainPassword)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, plainPassword);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        public User? GetUserByEmail(string email)
        {
            return LoadUsers().FirstOrDefault(u => u.Email == email);
        }
    }
}
