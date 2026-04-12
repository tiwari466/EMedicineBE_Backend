using Microsoft.Data.SqlClient;
using System.Data;

namespace EMedicineBE.Data.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _cs;

        public InvoiceRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("SqlServerCS");
        }

        public DataSet GetInvoiceData(int userId, int orderId)
        {
            DataSet ds = new();

            using var con = new SqlConnection(_cs);

            // 🔹 Invoice Header Query
            string headerQuery = @"
                SELECT 
                    o.id,
                    o.order_no,
                    o.order_total,
                    o.order_status,
                    o.placed_time,
                    u.first_name,
                    u.last_name,
                    u.email
                FROM cfg_set_order o
                JOIN cfg_set_user u ON u.user_id = o.user_id
                WHERE o.id = @orderId AND o.user_id = @userId";

            // 🔹 Invoice Items Query
            string itemsQuery = @"
                SELECT 
                    oi.id,
                    oi.medicine_id,
                    m.medicine_name,
                    oi.unit_price,
                    oi.discount,
                    oi.qty,
                    oi.total_price
                FROM cfg_set_order_item oi
                JOIN cfg_set_medicine m ON m.id = oi.medicine_id
                WHERE oi.order_id = @orderId AND oi.user_id = @userId";

            using var hCmd = new SqlCommand(headerQuery, con);
            hCmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
            hCmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

            using var iCmd = new SqlCommand(itemsQuery, con);
            iCmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
            iCmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

            con.Open();

            using (var da = new SqlDataAdapter(hCmd))
            {
                da.Fill(ds, "InvoiceHeader");
            }

            using (var da = new SqlDataAdapter(iCmd))
            {
                da.Fill(ds, "InvoiceItems");
            }

            return ds;
        }
    }
}