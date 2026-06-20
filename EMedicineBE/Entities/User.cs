namespace EMedicineBE.Entities
{
    public class User
    {
        public int user_id { get; set; }

        public string first_name { get; set; } = string.Empty;

        public string last_name { get; set; } = string.Empty;

        // Legacy column
        public string? password { get; set; }

        // BCrypt
        public string? password_hash { get; set; }

        public string? password_salt { get; set; }

        public string email { get; set; } = string.Empty;

        public decimal fund { get; set; }

        public string? type { get; set; }

        public string? status { get; set; }

        public string? picture { get; set; }

        public DateTime create_time { get; set; }

        public string role { get; set; } = "User";

        public bool is_active { get; set; }

        public bool email_verified { get; set; }

        public string? refresh_token { get; set; }

        public DateTime? refresh_token_expiry { get; set; }

        public DateTime created_at { get; set; }

        public DateTime? updated_at { get; set; }
    }
}