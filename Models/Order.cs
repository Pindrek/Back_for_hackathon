namespace Back_for_hackathon.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public decimal Subtotal { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        // computed
        public decimal CompositeTaxRate { get; set; } // e.g. 0.08875
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // breakdown
        public decimal StateRate { get; set; }
        public decimal CountyRate { get; set; }
        public decimal CityRate { get; set; }

        // store as JSON text to keep MVP simple
        public string SpecialRatesJson { get; set; } = "[]";
    }
}
