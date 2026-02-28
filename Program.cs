using Back_for_hackathon.Data;
using Back_for_hackathon.Services;
using Back_for_hackathon.Tax;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    opt.UseNpgsql(cs);
});

// Tax config
var taxCfg = new TaxZoneConfig();
builder.Configuration.GetSection("Tax").Bind(taxCfg);
builder.Services.AddSingleton(taxCfg);

// Services (у вас ці інтерфейси/класи оголошені в TaxRateService.cs і OrderService.cs)
builder.Services.AddSingleton<ITaxRateService, TaxRateService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// ===== HARDCODE AUTH (hackathon mode) =====
const string HARD_TOKEN = "hackathon-super-token";

app.Use(async (ctx, next) =>
{
    // Дозволяємо логін без токена
    if (ctx.Request.Path.Equals("/api/login", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    // Все інше — тільки Bearer token
    var auth = ctx.Request.Headers.Authorization.ToString();
    if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync("{\"message\":\"Auth required\"}");
        return;
    }

    var token = auth["Bearer ".Length..].Trim();
    if (token != HARD_TOKEN)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync("{\"message\":\"Invalid token\"}");
        return;
    }

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
