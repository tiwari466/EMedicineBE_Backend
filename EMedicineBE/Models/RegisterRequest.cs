namespace EMedicineBE.Models
{
    public class RegisterRequest
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }

        // optional
        public string? picture { get; set; }
    }
}
