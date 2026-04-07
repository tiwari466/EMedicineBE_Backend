using Npgsql;
using System.Data;

namespace EMedicineBE.Data.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _cs;

        public InvoiceRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("PostgresCS");
        }

        public DataSet GetInvoiceData(int userId, int orderId)
        {
            DataSet ds = new();
            using var con = new NpgsqlConnection(_cs);

            using var hCmd = new NpgsqlCommand(
                "SELECT * FROM invoice_header_fn(@u,@o)", con);
            hCmd.Parameters.AddWithValue("@u", userId);
            hCmd.Parameters.AddWithValue("@o", orderId);

            using var iCmd = new NpgsqlCommand(
                "SELECT * FROM invoice_items_fn(@u,@o)", con);
            iCmd.Parameters.AddWithValue("@u", userId);
            iCmd.Parameters.AddWithValue("@o", orderId);

            con.Open();
            new NpgsqlDataAdapter(hCmd).Fill(ds, "InvoiceHeader");
            new NpgsqlDataAdapter(iCmd).Fill(ds, "InvoiceItems");

            return ds;
        }
    }
}
