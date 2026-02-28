using Back_for_hackathon.Data;
using Back_for_hackathon.Dtos;
using Back_for_hackathon.Models;
using Back_for_hackathon.Tax;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Back_for_hackathon.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateAsync(OrderCreateRequest req, CancellationToken ct);
        Task<List<OrderResponse>> ImportCsvAsync(IFormFile file, CancellationToken ct);
        Task<PagedResult<OrderResponse>> GetAsync(
            int page, int pageSize,
            DateTimeOffset? from, DateTimeOffset? to,
            decimal? minSubtotal, decimal? maxSubtotal,
            CancellationToken ct);
    }

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly ITaxRateService _tax;
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        public OrderService(AppDbContext db, ITaxRateService tax)
        {
            _db = db;
            _tax = tax;
        }

        public async Task<OrderResponse> CreateAsync(OrderCreateRequest req, CancellationToken ct)
        {
            var breakdown = _tax.Resolve(req.Latitude, req.Longitude);

            var taxAmount = RoundMoney(req.Subtotal * breakdown.Composite);
            var total = RoundMoney(req.Subtotal + taxAmount);

            var order = new Order
            {
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Subtotal = RoundMoney(req.Subtotal),
                Timestamp = req.Timestamp,

                CompositeTaxRate = breakdown.Composite,
                TaxAmount = taxAmount,
                TotalAmount = total,

                StateRate = breakdown.State,
                CountyRate = breakdown.County,
                CityRate = breakdown.City,
                SpecialRatesJson = JsonSerializer.Serialize(breakdown.Specials, JsonOpts)
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            return ToResponse(order);
        }

        public async Task<List<OrderResponse>> ImportCsvAsync(IFormFile file, CancellationToken ct)
        {
            if (file.Length == 0) return new List<OrderResponse>();

            // CSV format expected columns:
            // latitude,longitude,subtotal,timestamp
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var created = new List<OrderResponse>();

            string? header = await reader.ReadLineAsync(ct);
            if (header is null) return created;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                if (!double.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, out var lat)) continue;
                if (!double.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out var lon)) continue;
                if (!decimal.TryParse(parts[2], System.Globalization.CultureInfo.InvariantCulture, out var subtotal)) continue;
                if (!DateTimeOffset.TryParse(parts[3], out var ts)) continue;

                var resp = await CreateAsync(new OrderCreateRequest
                {
                    Latitude = lat,
                    Longitude = lon,
                    Subtotal = subtotal,
                    Timestamp = ts
                }, ct);

                created.Add(resp);
            }

            return created;
        }
        public async Task<PagedResult<OrderResponse>> GetAsync(
       int page, int pageSize,
       DateTimeOffset? from, DateTimeOffset? to,
       decimal? minSubtotal, decimal? maxSubtotal,
       CancellationToken ct)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            IQueryable<Order> q = _db.Orders.AsNoTracking();

            if (from.HasValue) q = q.Where(x => x.Timestamp >= from.Value);
            if (to.HasValue) q = q.Where(x => x.Timestamp <= to.Value);
            if (minSubtotal.HasValue) q = q.Where(x => x.Subtotal >= minSubtotal.Value);
            if (maxSubtotal.HasValue) q = q.Where(x => x.Subtotal <= maxSubtotal.Value);

            var total = await q.LongCountAsync(ct);

            var items = await q
                .OrderByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<OrderResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = items.Select(ToResponse).ToList()
            };
        }
        private static decimal RoundMoney(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static OrderResponse ToResponse(Order o)
        {
            var specials = JsonSerializer.Deserialize<List<SpecialRate>>(o.SpecialRatesJson, JsonOpts) ?? new();

            return new OrderResponse
            {
                Id = o.Id,
                Latitude = o.Latitude,
                Longitude = o.Longitude,
                Subtotal = o.Subtotal,
                Timestamp = o.Timestamp,
                CompositeTaxRate = o.CompositeTaxRate,
                TaxAmount = o.TaxAmount,
                TotalAmount = o.TotalAmount,
                Breakdown = new()
                {
                    StateRate = o.StateRate,
                    CountyRate = o.CountyRate,
                    CityRate = o.CityRate,
                    SpecialRates = specials.Select(s => new SpecialRateDto { Name = s.Name, Rate = s.Rate }).ToList()
                }
            };
        }
    }
}