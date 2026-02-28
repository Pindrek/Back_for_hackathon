using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Якщо у вас вже є DbContext — залишайте як було.
// Я тут роблю "як у вас типово": з ConnectionStrings:Default
// Якщо у вас інша назва — замініть.
var conn = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrWhiteSpace(conn))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(conn));
}

// Якщо у вас є DI для сервісів/репозиторіїв — лишайте як було, тут не чіпаю.

var app = builder.Build();

// ===== Hardcoded auth data (hackathon mode) =====
const string HARD_LOGIN = "admin";
const string HARD_PASSWORD = "1234";
const string HARD_TOKEN = "hackathon-super-token";

// ===== Auth middleware: blocks everything except /api/login =====
app.Use(async (context, next) =>
{
    // 1) Логін доступний без токена
    if (context.Request.Path.Equals("/api/login", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    // 2) Якщо хочеш залишити Swagger відкритим — розкоментуй:
    // if (context.Request.Path.StartsWithSegments("/swagger")) { await next(); return; }

    // 3) Все інше — тільки з Bearer токеном
    var auth = context.Request.Headers.Authorization.ToString();
    if (string.IsNullOrWhiteSpace(auth) ||
        !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"Auth required\"}");
        return;
    }

    var token = auth["Bearer ".Length..].Trim();
    if (token != HARD_TOKEN)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"Invalid token\"}");
        return;
    }

    await next();
});

// ===== Pipeline =====
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();


// =========================
// Нижче потрібні типи, щоб Program.cs компілився,
// якщо у вас AppDbContext в іншому файлі — ВИДАЛИ цей блок.
// =========================
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
