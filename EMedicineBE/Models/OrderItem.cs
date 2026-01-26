namespace EMedicineBE.Models
{
    public class OrderItem
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int order_id { get; set; }
        public int medicine_id { get; set; }
        public decimal unit_price { get;set; }
        public decimal discount { get; set; }
        public int qty { get; set; }
        public decimal total_price { get; set; }


        
    }
}
