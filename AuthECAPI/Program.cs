using AuthECAPI.Controllers;
using AuthECAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthECAPI", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token like this: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Identity
builder.Services
    .AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT
var jwtSecret = builder.Configuration["AppSettings:JWTSecret"]
    ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthECAPI V1");
    });
}

app.UseHttpsRedirection();

// Add static files support for HTML pages
app.UseStaticFiles();

app.UseCors("AllowAngular");

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure endpoints - Authentication endpoints without RequireAuthorization
app.MapGroup("/api/users")
   .MapIdentityUserEndpoints()
   .WithTags("Users");
// ===== Add your protected route HERE =====
app.MapGet("/reports", [Authorize(Roles = "Admin")] () =>
    Results.Redirect("/auth/admin/reports.html"))
    .WithTags("Static Pages")
    .RequireAuthorization();

// Add default route to serve login page
app.MapGet("/", () => Results.Redirect("/auth/login.html"));
app.MapGet("/dashboard", () => Results.Redirect("/auth/dashboard.html"));
// Static routes for perfumes HTML pages
app.MapGet("/perfumes/edit", () => Results.Redirect("/auth/perfumes/edit-perfume.html"));
app.MapGet("/perfumes/catalog", () => Results.Redirect("/auth/perfumes/catalog.html"));
app.MapGet("/perfumes/create", () => Results.Redirect("/auth/perfumes/create-perfume.html"));
// Static routes for orders HTML pages
app.MapGet("/orders/list", () => Results.Redirect("/auth/orders/orders.html"));
app.MapGet("/orders/details", () => Results.Redirect("/auth/orders/order-details.html"));
app.MapGet("/orders/custom", () => Results.Redirect("/auth/orders/create-custom-order.html"));
// Add reports page route
app.MapGet("/reports", () => Results.Redirect("/auth/admin/reports.html"));



app.Run();