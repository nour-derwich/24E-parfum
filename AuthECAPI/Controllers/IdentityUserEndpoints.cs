using AuthECAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthECAPI.Controllers
{
    public static class IdentityUserEndpoints
    {
        public static RouteGroupBuilder MapIdentityUserEndpoints(this RouteGroupBuilder group)
        {
            // Endpoint d'inscription
            group.MapPost("/signup", async (
                [FromBody] UserRegistrationModel model,
                UserManager<AppUser> userManager) =>
            {
                // Validation du modèle
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    return Results.BadRequest("Email and password are required");
                }

                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    UserRole = model.Role ?? "Client" // Valeur par défaut sécurisée
                };

                var result = await userManager.CreateAsync(user, model.Password);

                return result.Succeeded
                    ? Results.Ok(new { user.Id, user.Email, user.FullName, user.UserRole })
                    : Results.BadRequest(result.Errors);
            }).AllowAnonymous()
              .WithTags("Authentication");

            // Endpoint de connexion
            group.MapPost("/signin", async (
                [FromBody] LoginModel model,
                UserManager<AppUser> userManager,
                IConfiguration configuration) =>
            {
                // Validation des entrées
                if (string.IsNullOrEmpty(model.Email))
                    return Results.BadRequest("Email is required");

                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
                    return Results.Unauthorized();

                // Génération du JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(
                    configuration["AppSettings:JWTSecret"]
                    ?? throw new InvalidOperationException("JWT Secret not configured"));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim("fullName", user.FullName),
                        new Claim(ClaimTypes.Role, user.UserRole)
                    }),
                    Expires = DateTime.UtcNow.AddHours(2), // Durée de vie plus courte
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Results.Ok(new
                {
                    Token = tokenHandler.WriteToken(token),
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.UserRole
                });
            }).WithTags("Authentication");
            /* Get All Users*/
             group.MapGet("/users", static async (
                  UserManager<AppUser> userManager,
               [FromQuery] int page = 1,
               [FromQuery] int pageSize = 10
               ) =>
                {
                    var users = userManager.Users
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(u => new
                        {
                            u.Id,
                            u.Email,
                            u.FullName,
                            u.UserRole,
                            EmailConfirmed = u.EmailConfirmed
                        });

                    return Results.Ok(users);
                })
                .RequireAuthorization()
                .WithTags("User Management");
            return group;
        }
    }

    // Modèles DTO (à déplacer dans un dossier séparé si possible)
    public class UserRegistrationModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }

    public class LoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}