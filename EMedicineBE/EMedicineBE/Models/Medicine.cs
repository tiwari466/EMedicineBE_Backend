namespace EMedicineBE.Models
{
    public class Medicine
    {
        public int id { get; set; }
        public string medicine_name { get; set; }
        public string manufacturer { get; set; }
        public decimal unit_price { get; set; }
        public decimal discount { get; set; }
        public int qty { get; set; }
        public string disease  { get; set; }
        public string uses { get; set; }
        public DateTime exp_date { get; set; }
        public string image_url { get; set; }
        public string? status { get;set; }
        public string type { get; set; }
    }
}
