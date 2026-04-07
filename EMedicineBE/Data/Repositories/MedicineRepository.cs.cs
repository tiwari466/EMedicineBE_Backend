using EMedicineBE.Entities;
using Npgsql;

namespace EMedicineBE.Data.Repositories
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly string _cs;

        public MedicineRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("PostgresCS");
        }

        public async Task<bool> SaveAsync(Medicine m)
        {
            using var con = new NpgsqlConnection(_cs);

            string sql = m.id == 0
                ? @"INSERT INTO cfg_set_medicine
                   (medicine_name, manufacturer, unit_price, discount, qty, disease, uses, exp_date, image_url, status, type)
                   VALUES (@n,@man,@up,@d,@q,@dis,@u,@exp,@img,@st,@t)"
                : @"UPDATE cfg_set_medicine SET
                   medicine_name=@n, manufacturer=@man, unit_price=@up,
                   discount=@d, qty=@q, disease=@dis, uses=@u,
                   exp_date=@exp, image_url=@img, status=@st, type=@t
                   WHERE id=@id";

            using var cmd = new NpgsqlCommand(sql, con);

            if (m.id > 0)
                cmd.Parameters.AddWithValue("@id", m.id);

            cmd.Parameters.AddWithValue("@n", m.medicine_name);
            cmd.Parameters.AddWithValue("@man", m.manufacturer);
            cmd.Parameters.AddWithValue("@up", m.unit_price);
            cmd.Parameters.AddWithValue("@d", m.discount);
            cmd.Parameters.AddWithValue("@q", m.qty);
            cmd.Parameters.AddWithValue("@dis", (object?)m.disease ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", (object?)m.uses ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@exp", m.exp_date);
            cmd.Parameters.AddWithValue("@img", (object?)m.image_url ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@st", m.status);
            cmd.Parameters.AddWithValue("@t", m.type);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Medicine>> GetAllAsync()
        {
            var list = new List<Medicine>();
            using var con = new NpgsqlConnection(_cs);

            using var cmd = new NpgsqlCommand(
                "SELECT * FROM cfg_set_medicine ORDER BY id DESC", con);

            await con.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                list.Add(new Medicine
                {
                    id = r.GetInt32(r.GetOrdinal("id")),
                    medicine_name = r.GetString(r.GetOrdinal("medicine_name")),
                    manufacturer = r.GetString(r.GetOrdinal("manufacturer")),
                    unit_price = r.GetDecimal(r.GetOrdinal("unit_price")),
                    discount = r.GetDecimal(r.GetOrdinal("discount")),
                    qty = r.GetInt32(r.GetOrdinal("qty")),
                    disease = r.IsDBNull(r.GetOrdinal("disease")) ? null : r.GetString(r.GetOrdinal("disease")),
                    uses = r.IsDBNull(r.GetOrdinal("uses")) ? null : r.GetString(r.GetOrdinal("uses")),
                    exp_date = r.GetDateTime(r.GetOrdinal("exp_date")),
                    image_url = r.IsDBNull(r.GetOrdinal("image_url")) ? null : r.GetString(r.GetOrdinal("image_url")),
                    status = r.IsDBNull(r.GetOrdinal("status")) ? null : r.GetString(r.GetOrdinal("status"))
                });
            }

            return list;
        }
    }
}
