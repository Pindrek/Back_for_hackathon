namespace Back_for_hackathon.Dtos
{
    public class OrderCreateRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal Subtotal { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
