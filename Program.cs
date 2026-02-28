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

// Tax config bind
var taxCfg = new TaxZoneConfig();
builder.Configuration.GetSection("Tax").Bind(taxCfg);
builder.Services.AddSingleton(taxCfg);

// Services
builder.Services.AddSingleton<ITaxRateService, TaxRateService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();