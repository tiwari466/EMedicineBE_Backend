namespace EMedicineBE.Dto.Auth
{
    public class RegisterRequestDto
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }

        // optional
        public string? picture { get; set; }
    }
}
