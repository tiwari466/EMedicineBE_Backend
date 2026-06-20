namespace EMedicineBE.Dto.Auth
{
    public class LoginResponseDto
    {
        public int user_id { get; set; }

        public string email { get; set; } = string.Empty;

        public string role { get; set; } = string.Empty;

        public string token { get; set; } = string.Empty;

        public string? picture { get; set; }
    }
}