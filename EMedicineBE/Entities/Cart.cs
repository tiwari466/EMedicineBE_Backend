namespace EMedicineBE.Entities
{
    public class Cart
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int medicine_id { get; set; }
        public decimal unit_price { get; set; }

        public decimal discount { get; set; }

        public int qty { get; set; }
        public decimal total_price { get; set; }
        public string? medicine_name { get; set; }
        public string?image_url { get; set; }
        }
}
