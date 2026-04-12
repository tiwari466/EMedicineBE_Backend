using EMedicineBE.Entities;
using Microsoft.Data.SqlClient;

namespace EMedicineBE.Data.Repositories
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly string _cs;

        public MedicineRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("SqlServerCS");
        }

        public async Task<bool> SaveAsync(Medicine m)
        {
            using var con = new SqlConnection(_cs);

            string sql = m.id == 0
                ? @"INSERT INTO cfg_set_medicine
                   (medicine_name, manufacturer, unit_price, discount, qty, disease, uses, exp_date, image_url, status, type)
                   VALUES (@n,@man,@up,@d,@q,@dis,@u,@exp,@img,@st,@t)"
                : @"UPDATE cfg_set_medicine SET
                   medicine_name=@n, manufacturer=@man, unit_price=@up,
                   discount=@d, qty=@q, disease=@dis, uses=@u,
                   exp_date=@exp, image_url=@img, status=@st, type=@t
                   WHERE id=@id";

            using var cmd = new SqlCommand(sql, con);

            if (m.id > 0)
                cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = m.id;

            cmd.Parameters.Add("@n", System.Data.SqlDbType.NVarChar).Value = m.medicine_name ?? (object)DBNull.Value;
            cmd.Parameters.Add("@man", System.Data.SqlDbType.NVarChar).Value = m.manufacturer ?? (object)DBNull.Value;
            cmd.Parameters.Add("@up", System.Data.SqlDbType.Decimal).Value = m.unit_price;
            cmd.Parameters.Add("@d", System.Data.SqlDbType.Decimal).Value = m.discount;
            cmd.Parameters.Add("@q", System.Data.SqlDbType.Int).Value = m.qty;
            cmd.Parameters.Add("@dis", System.Data.SqlDbType.NVarChar).Value = (object?)m.disease ?? DBNull.Value;
            cmd.Parameters.Add("@u", System.Data.SqlDbType.NVarChar).Value = (object?)m.uses ?? DBNull.Value;
            cmd.Parameters.Add("@exp", System.Data.SqlDbType.DateTime).Value = m.exp_date;
            cmd.Parameters.Add("@img", System.Data.SqlDbType.NVarChar).Value = (object?)m.image_url ?? DBNull.Value;
            cmd.Parameters.Add("@st", System.Data.SqlDbType.NVarChar).Value = (object?)m.status ?? DBNull.Value;
            cmd.Parameters.Add("@t", System.Data.SqlDbType.NVarChar).Value = (object?)m.type ?? DBNull.Value;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Medicine>> GetAllAsync()
        {
            var list = new List<Medicine>();

            using var con = new SqlConnection(_cs);

            using var cmd = new SqlCommand(
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
                    status = r.IsDBNull(r.GetOrdinal("status")) ? null : r.GetString(r.GetOrdinal("status")),
                    type = r.IsDBNull(r.GetOrdinal("type")) ? null : r.GetString(r.GetOrdinal("type"))
                });
            }

            return list;
        }
    }
}