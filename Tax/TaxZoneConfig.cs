namespace Back_for_hackathon.Tax
{
    public class TaxZoneConfig
    {
        public decimal StateRate { get; set; } = 0.04m;
        public List<TaxZone> Zones { get; set; } = new();
    }

    public class TaxZone
    {
        public string Name { get; set; } = "";
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLon { get; set; }

        public decimal CountyRate { get; set; }
        public decimal CityRate { get; set; }

        public List<SpecialRate> SpecialRates { get; set; } = new();
    }

    public class SpecialRate
    {
        public string Name { get; set; } = "";
        public decimal Rate { get; set; }
    }
}
