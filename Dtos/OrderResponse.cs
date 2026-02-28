namespace Back_for_hackathon.Dtos
{
    public class OrderResponse
    {
        public Guid Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public decimal Subtotal { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public decimal CompositeTaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public BreakdownDto Breakdown { get; set; } = new();
    }

    public class BreakdownDto
    {
        public decimal StateRate { get; set; }
        public decimal CountyRate { get; set; }
        public decimal CityRate { get; set; }
        public List<SpecialRateDto> SpecialRates { get; set; } = new();
    }

    public class SpecialRateDto
    {
        public string Name { get; set; } = "";
        public decimal Rate { get; set; }
    }
}
