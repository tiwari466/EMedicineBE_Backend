namespace EMedicineBE.Models
{
    public class Order
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string order_no { get; set; }
        public decimal order_total { get; set; }

        public string order_status { get; set; }
    }
}
