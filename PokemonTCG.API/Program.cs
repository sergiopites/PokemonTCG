using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Configuración de Serilog
// --------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
    .CreateLogger();

builder.Host.UseSerilog();

// --------------------
// Configuración de servicios
// --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ CORS para localhost y 127.0.0.1
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 🔹 Inyección de dependencias
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ISetRepository, SetRepository>();
builder.Services.AddScoped<ISetService, SetService>();
builder.Services.AddScoped<ICardImageService, CardImageService>();
builder.Services.AddScoped<ICardImageRepository, CardImageRepository>();
builder.Services.AddScoped<ILegalityRepository, LegalityRepository>();
builder.Services.AddScoped<IAbilityRepository, AbilityRepository>();
builder.Services.AddScoped<IAttackRepository, AttackRepository>();
builder.Services.AddScoped<ICardMarketRepository, CardMarketRepository>();
builder.Services.AddScoped<ICardMarketService, CardMarketService>();
builder.Services.AddScoped<ITCGPlayerRepository, TCGPlayerRepository>();
builder.Services.AddScoped<ITCGPlayerService, TCGPlayerService>();
builder.Services.AddScoped<IAncientTraitRepository, AncientTraitRepository>();
builder.Services.AddScoped<IAncientTraitService, AncientTraitService>();
builder.Services.AddScoped<IAttackService, AttackService>();
builder.Services.AddScoped<IAbilityService, AbilityService>();
builder.Services.AddScoped<IResistanceService, ResistanceService>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<IResistanceRepository, ResistanceRepository>();
builder.Services.AddScoped<ISetImageRepository, SetImageRepository>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --------------------
// Middleware pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ IMPORTANTE: CORS va ANTES de redirección HTTPS y autorización
app.UseCors(corsPolicy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
