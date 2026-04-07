namespace EMedicineBE.Entities
{
    public class User
    {
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string password { get; set; }   
        public string email { get; set; }
        public decimal fund { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string picture { get; set; }
        public DateTime create_time { get; set; }

    }
}
