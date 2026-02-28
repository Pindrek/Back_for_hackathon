using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ... ваші сервіси (controllers, swagger, db, etc.)

var app = builder.Build();

// ==== HARDcoded credentials (для хакатону ок, для прод — ні) ====
const string HARD_LOGIN = "admin";
const string HARD_PASSWORD = "1234";

// ==== Basic Auth middleware ====
app.Use(async (context, next) =>
{
    // Якщо хочеш лишити health/public — тут можна зробити whitelist по path
    // Наприклад:
    // if (context.Request.Path.StartsWithSegments("/health")) { await next(); return; }

    if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Back_for_hackathon\"";
        await context.Response.WriteAsync("Auth required");
        return;
    }

    var auth = authHeader.ToString();
    if (!auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Back_for_hackathon\"";
        await context.Response.WriteAsync("Invalid auth scheme");
        return;
    }

    string decoded;
    try
    {
        var encoded = auth["Basic ".Length..].Trim();
        decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
    }
    catch
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Back_for_hackathon\"";
        await context.Response.WriteAsync("Bad auth header");
        return;
    }

    // Формат: login:password
    var parts = decoded.Split(':', 2);
    if (parts.Length != 2)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Back_for_hackathon\"";
        await context.Response.WriteAsync("Bad credentials format");
        return;
    }

    var login = parts[0];
    var pass = parts[1];

    if (login != HARD_LOGIN || pass != HARD_PASSWORD)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Back_for_hackathon\"";
        await context.Response.WriteAsync("Wrong login/password");
        return;
    }

    await next();
});

// Далі ваш пайплайн:
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
