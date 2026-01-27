namespace EMedicineBE.Models
{
    public class Order
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string order_no { get; set; }
        public decimal order_total { get; set; }
        public string order_status { get; set; }
        public DateTime? placed_time { get; set; }
        public DateTime? shipped_time { get; set; }
        public DateTime? out_for_delivery_time { get; set; }
        public DateTime? delivered_time { get; set; }

        public DateTime? expected_delivery_date { get; set; }
        public List<OrderItem> items { get; set; } = new List<OrderItem>();
    }
}
