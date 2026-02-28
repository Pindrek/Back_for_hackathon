using Microsoft.AspNetCore.Mvc;

namespace Back_for_hackathon.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    // HARDCODED (потім заміниш на норм auth)
    private const string HARD_LOGIN = "admin";
    private const string HARD_PASSWORD = "1234";

    // Токен теж просто хардкод (мінімальний варіант)
    private const string HARD_TOKEN = "hackathon-super-token";

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (req is null) return BadRequest();

        // фронт шле { email, password } :contentReference[oaicite:2]{index=2}
        if (req.Email == HARD_LOGIN && req.Password == HARD_PASSWORD)
        {
            return Ok(new
            {
                token = HARD_TOKEN,
                user = new { email = req.Email }
            });
        }

        return Unauthorized(new { message = "Invalid login or password" });
    }
}
