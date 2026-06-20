using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMedicineBE.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _cs;

        public UserRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("SqlServerCS");
        }

        // 🔹 REGISTER
        public async Task<int> Register(RegisterRequestDto dto)
        {
            using var con = new SqlConnection(_cs);

            // Check if email exists
            string checkSql = "SELECT COUNT(1) FROM cfg_set_user WHERE email = @em";

            using (var checkCmd = new SqlCommand(checkSql, con))
            {
                checkCmd.Parameters.Add("@em", SqlDbType.NVarChar).Value = dto.email;

                await con.OpenAsync();

                int exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (exists > 0)
                    throw new Exception("Email already registered");

                await con.CloseAsync();
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.password);

            string sql = @"
    INSERT INTO cfg_set_user
    (
        first_name,
        last_name,
        email,
        password_hash,
        role,
        fund,
        type,
        status,
        picture,
        email_verified,
        is_active,
        created_at
    )
    VALUES
    (
        @fn,
        @ln,
        @em,
        @ph,
        'User',
        0,
        'Users',
        'Pending',
        @pic,
        0,
        1,
        GETDATE()
    )";

            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@fn", SqlDbType.NVarChar).Value = dto.first_name ?? "";
            cmd.Parameters.Add("@ln", SqlDbType.NVarChar).Value = dto.last_name ?? "";
            cmd.Parameters.Add("@em", SqlDbType.NVarChar).Value = dto.email ?? "";
            cmd.Parameters.Add("@ph", SqlDbType.NVarChar).Value = passwordHash;
            cmd.Parameters.Add("@pic", SqlDbType.NVarChar).Value =
                (object?)dto.picture ?? DBNull.Value;

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync();
        }
        // 🔹 LOGIN
        public async Task<User?> Login(string email)
        {
            using var con = new SqlConnection(_cs);

            string sql = @"
    SELECT
        user_id,
        first_name,
        last_name,
        email,
        type,
        picture,
        password_hash,
        role,
        is_active
    FROM cfg_set_user
    WHERE email = @em";

            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@em", SqlDbType.NVarChar).Value = email;

            await con.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();

            if (!r.Read())
                return null;

            return new User
            {
                user_id = Convert.ToInt32(r["user_id"]),
                first_name = r["first_name"]?.ToString() ?? "",
                last_name = r["last_name"]?.ToString() ?? "",
                email = r["email"]?.ToString() ?? "",
                type = r["type"]?.ToString(),
                picture = r["picture"]?.ToString(),
                password_hash = r["password_hash"]?.ToString(),
                role = r["role"]?.ToString() ?? "User",
                is_active = Convert.ToBoolean(r["is_active"])
            };
        }
        // 🔹 GET USER
        public async Task<User?> GetUser(int userId)
        {
            using var con = new SqlConnection(_cs);

            string sql = "SELECT * FROM cfg_set_user WHERE user_id=@id";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;

            await con.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();

            if (!r.Read()) return null;

            return new User
            {
                user_id = r.GetInt32(r.GetOrdinal("user_id")),
                first_name = r.GetString(r.GetOrdinal("first_name")),
                last_name = r.GetString(r.GetOrdinal("last_name")),
                email = r.GetString(r.GetOrdinal("email")),
                type = r.GetString(r.GetOrdinal("type")),
                fund = r.IsDBNull(r.GetOrdinal("fund")) ? 0 : r.GetDecimal(r.GetOrdinal("fund"))
            };
        }

        // 🔹 UPDATE PROFILE
        public async Task<bool> UpdateProfile(UpdateProfileRequestDto dto)
        {
            using var con = new SqlConnection(_cs);

            string sql;

            using var cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = dto.user_id;
            cmd.Parameters.Add("@fn", SqlDbType.NVarChar).Value = dto.first_name ?? "";
            cmd.Parameters.Add("@ln", SqlDbType.NVarChar).Value = dto.last_name ?? "";
            cmd.Parameters.Add("@em", SqlDbType.NVarChar).Value = dto.email ?? "";

            if (string.IsNullOrWhiteSpace(dto.password))
            {
                sql = @"
        UPDATE cfg_set_user
        SET first_name = @fn,
            last_name = @ln,
            email = @em,
            updated_at = GETDATE()
        WHERE user_id = @id";
            }
            else
            {
                string passwordHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.password);

                sql = @"
        UPDATE cfg_set_user
        SET first_name = @fn,
            last_name = @ln,
            email = @em,
            password_hash = @pw,
            updated_at = GETDATE()
        WHERE user_id = @id";

                cmd.Parameters.Add("@pw", SqlDbType.NVarChar)
                              .Value = passwordHash;
            }

            cmd.CommandText = sql;

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        // 🔹 UPDATE PROFILE PICTURE
        public async Task<bool> UpdateProfilePicture(int userId, string pictureUrl)
        {
            using var con = new SqlConnection(_cs);

            string sql = "UPDATE cfg_set_user SET picture=@pic WHERE user_id=@id";

            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@pic", SqlDbType.NVarChar).Value = pictureUrl;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // 🔹 GET USERS
        public async Task<List<User>> GetUsers()
        {
            var list = new List<User>();

            using var con = new SqlConnection(_cs);

            using var cmd = new SqlCommand(
                "SELECT * FROM cfg_set_user ORDER BY user_id DESC", con);

            await con.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                list.Add(new User
                {
                    user_id = r.GetInt32(r.GetOrdinal("user_id")),
                    first_name = r.GetString(r.GetOrdinal("first_name")),
                    last_name = r.GetString(r.GetOrdinal("last_name")),
                    email = r.GetString(r.GetOrdinal("email"))
                });
            }

            return list;
        }
    }
}