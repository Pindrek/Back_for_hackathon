using Back_for_hackathon.Dtos;
using Back_for_hackathon.Services;
using Microsoft.AspNetCore.Mvc;

namespace Back_for_hackathon.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders)
        {
            _orders = orders;
        }

        // POST /orders
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] OrderCreateRequest req, CancellationToken ct)
        {
            if (req.Subtotal < 0) return BadRequest("subtotal must be >= 0");

            var result = await _orders.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetList), new { id = result.Id }, result);
        }

        // POST /orders/import
        [HttpPost("import")]
        [RequestSizeLimit(50_000_000)]
        public async Task<ActionResult<List<OrderResponse>>> Import([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file is null) return BadRequest("file is required");
            var created = await _orders.ImportCsvAsync(file, ct);
            return Ok(created);
        }

        // GET /orders?page=1&pageSize=20&from=...&to=...&minSubtotal=...&maxSubtotal=...
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderResponse>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTimeOffset? from = null,
            [FromQuery] DateTimeOffset? to = null,
            [FromQuery] decimal? minSubtotal = null,
            [FromQuery] decimal? maxSubtotal = null,
            CancellationToken ct = default)
        {
            var result = await _orders.GetAsync(page, pageSize, from, to, minSubtotal, maxSubtotal, ct);
            return Ok(result);
        }
    }
}
