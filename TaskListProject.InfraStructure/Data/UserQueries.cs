using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskListProject.Infrastructure.Data
{
    public class UserQueries
    {
        private readonly TaskContext _context;

        // Keep parameterless constructor for tests or callers that rely on it.
        public UserQueries()
        {
            _context = GetContext();
        }

        // Constructor for dependency injection - allows the DbContext to be provided by the DI container.
        public UserQueries(TaskContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


       protected TaskContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
            var dbLocation = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["LocalDbConnection"];

            optionsBuilder.UseSqlServer(dbLocation);
            return new TaskContext(optionsBuilder.Options);
        }                

        // Changed: returns JWT token string when credentials are valid; null otherwise.
        public string GetUserPassword(string userLogin, string password)
        {
            var user = _context.UserLogins.Where(p => p.User == userLogin).FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            // Plain-text comparison (existing behavior). Replace with hashed compare in production.
            if (user.Password != password)
            {
                return null;
            }

            // Read JWT settings from appsettings.json
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var jwtSection = config.GetSection("Jwt");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException("JWT configuration (Jwt:Key / Issuer / Audience) missing in appsettings.json");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.User),
                new Claim("UserId", user.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }
        

    }
}
