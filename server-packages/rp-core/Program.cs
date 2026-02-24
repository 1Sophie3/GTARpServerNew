using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RPCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddSingleton<BankService>();

// Controller aktivieren
builder.Services.AddControllers();

var app = builder.Build();

// Routing für Controller
app.MapControllers();

app.Run();
