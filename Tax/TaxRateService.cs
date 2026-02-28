using System.Text.Json;
using Back_for_hackathon.Tax;

namespace Back_for_hackathon.Tax
{
    public record TaxBreakdown(
     decimal Composite,
     decimal State,
     decimal County,
     decimal City,
     List<SpecialRate> Specials
 );

    public interface ITaxRateService
    {
        TaxBreakdown Resolve(double latitude, double longitude);
    }
    public class TaxRateService : ITaxRateService
    {
        private readonly TaxZoneConfig _cfg;

        public TaxRateService(TaxZoneConfig cfg)
        {
            _cfg = cfg;
        }

        public TaxBreakdown Resolve(double latitude, double longitude)
        {
            // find first matching zone
            var zone = _cfg.Zones.FirstOrDefault(z =>
                latitude >= z.MinLat && latitude <= z.MaxLat &&
                longitude >= z.MinLon && longitude <= z.MaxLon
            );

            var state = _cfg.StateRate;
            var county = zone?.CountyRate ?? 0m;
            var city = zone?.CityRate ?? 0m;
            var specials = zone?.SpecialRates ?? new List<SpecialRate>();

            var specialSum = specials.Sum(s => s.Rate);
            var composite = state + county + city + specialSum;

            return new TaxBreakdown(composite, state, county, city, specials);
        }
    }
}
