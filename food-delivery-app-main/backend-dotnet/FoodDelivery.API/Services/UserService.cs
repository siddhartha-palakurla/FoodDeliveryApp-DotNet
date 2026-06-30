using FoodDelivery.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _jwtKey;

        public UserService(IMongoClient client, IConfiguration config)
        {
            var database = client.GetDatabase(
                config["MongoDb:DatabaseName"] ?? "food-delivery"
            );

            _users = database.GetCollection<User>("users");

            _jwtKey = config["Jwt:Key"]
                ?? throw new Exception("JWT Key missing in appsettings.json");
        }

        // 🔐 JWT creation (replaces createToken in Node)
        private string CreateToken(string userId)
        {
            var claims = new[]
            {
                new Claim("id", userId)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtKey)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🔹 Login
        public async Task<string?> Login(string email, string password)
        {
            var user = await _users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            var isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!isValid) return null;

            return CreateToken(user.Id);
        }

        // 🔹 Register
        public async Task<(bool Success, string Message, string? Token)> Register(
    string name,
    string email,
    string password)
        {
            // Check if user already exists
            var exists = await _users
                .Find(u => u.Email == email)
                .AnyAsync();

            if (exists)
                return (false, "User already exists", null);

            // Validate email
            if (!new EmailAddressAttribute().IsValid(email))
                return (false, "Please enter a valid email", null);

            // Validate password
            if (password.Length < 8)
                return (false, "Please enter a strong password", null);

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                CartData = new Dictionary<string, int>()
            };

            await _users.InsertOneAsync(user);

            var token = CreateToken(user.Id);

            return (true, "Registered Successfully", token);
        }

        // 🔹 Google Login
        public async Task<string> GoogleLogin(string name, string email, string image)
        {
            var user = await _users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    Image = image,
                    CartData = new Dictionary<string, int>()
                };

                await _users.InsertOneAsync(user);
            }

            return CreateToken(user.Id);
        }
    }
}
