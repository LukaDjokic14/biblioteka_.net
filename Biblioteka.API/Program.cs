using Biblioteka.API.Service;
using Biblioteka.Domain.Repozitorijumi;
using Biblioteka.Infrastructure;
using Biblioteka.Infrastructure.Repozitorijumi;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SVE builder.Services IDE OVDE ---
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Baza i Repozitorijumi
builder.Services.AddDbContext<BibliotekaContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IKnjigaRepository, KnjigaRepository>();
builder.Services.AddScoped<IClanRepository, ClanRepository>();
builder.Services.AddScoped<IIzdavanjeRepository, IzdavanjeRepository>();

// JWT Konfiguracija
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Biblioteka.API",
            ValidAudience = "Biblioteka.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("OvoJeNekiDovoljnoDugacakStringZaPotpisBiblioteka"))
        };
    });

builder.Services.AddScoped<IzdavanjeBackgroundService>();
builder.Services.AddHangfire(config => config.UseInMemoryStorage());
builder.Services.AddHangfireServer();

// OVO PREMESTI OVDE (pre builder.Build)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();


app.UseCors("AllowFrontend"); 
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire interfejs
app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<IzdavanjeBackgroundService>(
    "automatska-provera-izdavanja",
    servis => servis.ProveriStatusIzdavanja(),
    Cron.Minutely);

app.MapControllers();
app.Run();