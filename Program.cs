using System.Security.Claims;
using System.Text;
using JwtAspnet;
using JwtAspnet.Extensions;
using JwtAspnet.Models;
using JwtAspnet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Authentication and Authorization
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.PrivateKey)),
        ValidateIssuer =false,
        ValidateAudience = false,
    };
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("admin", policy => policy.RequireRole("admin"));
});

builder.Services.AddTransient<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use Authentication Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapGet("/", (TokenService service) =>
{
    var user = new User(1, "Cleber", "cleber@gmail.com", "image-teste", "teste123", ["student", "admin"]);
    return service.CreateToken(user);
});

app.MapGet("/restricted", (ClaimsPrincipal user) => new
{
    id = user.GetId(),
    name = user.GetName(),
    email = user.GetEmail(),
    giveName = user.GivenName(),
    image = user.GetImage(),
})
    .RequireAuthorization();

app.MapGet("/admin", () => "Access allowed!").RequireAuthorization("admin");

app.Run();