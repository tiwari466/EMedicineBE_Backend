namespace EMedicineBE.Models
{
    public class UpdateProfileRequest
    {
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string picture { get; set; }
    }
}
