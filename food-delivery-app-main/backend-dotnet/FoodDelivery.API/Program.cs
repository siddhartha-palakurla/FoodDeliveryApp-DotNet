using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using Microsoft.Extensions.FileProviders;
using FoodDelivery.API.Services;   // 🔹 REQUIRED

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add Controllers
builder.Services.AddControllers();

// 🔹 MongoDB connection (REQUIRED for UserService)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDb:ConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new Exception("MongoDB connection string missing in appsettings.json");

    return new MongoClient(connectionString);
});

// 🔹 Read JWT key safely
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("JWT Key is missing in appsettings.json");
}

// 🔹 JWT Authentication (replaces authMiddleware.js)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),
        NameClaimType = "id"
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Not Authorized Login Again\"}"
            );
        }
    };
});

// 🔹 Authorization
builder.Services.AddAuthorization();

// 🔹 Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<FoodService>();
builder.Services.AddScoped<OrderService>();

Stripe.StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",// frontend
                    "http://localhost:5174") // admin
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


var app = builder.Build();

// 🔴 REQUIRED
app.UseRouting();

app.UseCors("AllowReactApp");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")
    ),
    RequestPath = "/images"
});


// 🔹 Middleware pipeline
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Map controllers
app.MapControllers();

app.Run();
