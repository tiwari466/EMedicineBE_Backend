using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;
using Npgsql;

namespace EMedicineBE.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _cs;

        public UserRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("PostgresCS");
        }

        public async Task<int> Register(RegisterRequestDto dto)
        {
            using var con = new NpgsqlConnection(_cs);
            string sql = @"INSERT INTO cfg_set_user
                (first_name,last_name,password,email,fund,type,status,picture)
                VALUES (@fn,@ln,@pw,@em,0,'Users','Pending',@pic)";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@fn", dto.first_name);
            cmd.Parameters.AddWithValue("@ln", dto.last_name);
            cmd.Parameters.AddWithValue("@pw", dto.password);
            cmd.Parameters.AddWithValue("@em", dto.email);
            cmd.Parameters.AddWithValue("@pic", (object?)dto.picture ?? DBNull.Value);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<User?> Login(string email, string password)
        {
            using var con = new NpgsqlConnection(_cs);
            string sql = @"SELECT user_id,first_name,last_name,email,type,picture
                           FROM cfg_set_user
                           WHERE email=@em AND password=@pw";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@em", email);
            cmd.Parameters.AddWithValue("@pw", password);

            await con.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            if (!r.Read()) return null;

            return new User
            {
                user_id = r.GetInt32(0),
                first_name = r.GetString(1),
                last_name = r.GetString(2),
                email = r.GetString(3),
                type = r.GetString(4),
                picture = r.IsDBNull(5) ? null : r.GetString(5)
            };
        }

        public async Task<User?> GetUser(int userId)
        {
            using var con = new NpgsqlConnection(_cs);
            string sql = "SELECT * FROM cfg_set_user WHERE user_id=@id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", userId);

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

        public async Task<bool> UpdateProfile(UpdateProfileRequestDto dto)
        {
            using var con = new NpgsqlConnection(_cs);

            string sql;

            // If password is empty → don't update it
            if (string.IsNullOrWhiteSpace(dto.password))
            {
                sql = @"
            UPDATE cfg_set_user 
            SET first_name = @fn,
                last_name  = @ln,
                email      = @em
            WHERE user_id = @id
        ";
            }
            else
            {
                sql = @"
            UPDATE cfg_set_user 
            SET first_name = @fn,
                last_name  = @ln,
                email      = @em,
                password   = @pw
            WHERE user_id = @id
        ";
            }

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@id", dto.user_id);
            cmd.Parameters.AddWithValue("@fn", dto.first_name ?? "");
            cmd.Parameters.AddWithValue("@ln", dto.last_name ?? "");
            cmd.Parameters.AddWithValue("@em", dto.email ?? "");

            // Add password only if exists
            if (!string.IsNullOrWhiteSpace(dto.password))
            {
                cmd.Parameters.AddWithValue("@pw", dto.password);
            }

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync() > 0;
        }


        public async Task<bool> UpdateProfilePicture(int userId, string pictureUrl)
        {
            using var con = new NpgsqlConnection(_cs);
            string sql = "UPDATE cfg_set_user SET picture=@pic WHERE user_id=@id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@pic", pictureUrl);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<User>> GetUsers()
        {
            var list = new List<User>();
            using var con = new NpgsqlConnection(_cs);
            using var cmd = new NpgsqlCommand("SELECT * FROM cfg_set_user ORDER BY user_id DESC", con);

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
