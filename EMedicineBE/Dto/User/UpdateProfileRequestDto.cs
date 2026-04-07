namespace EMedicineBE.Dto.User
{
    public class UpdateProfileRequestDto
    {
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string? password { get; set; }
    }
}
