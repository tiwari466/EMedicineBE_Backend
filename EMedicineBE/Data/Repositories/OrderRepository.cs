using EMedicineBE.Dto.Order;
using EMedicineBE.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMedicineBE.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _cs;

        public OrderRepository(IConfiguration configuration)
        {
            _cs = configuration.GetConnectionString("SqlServerCS");
        }

        // 🔹 PLACE ORDER
        public async Task<bool> PlaceOrderAsync(PlaceOrderDto dto)
        {
            using var con = new SqlConnection(_cs);
            await con.OpenAsync();

            using var tx = con.BeginTransaction();

            try
            {
                // 1️⃣ Check cart
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM cfg_set_cart WHERE user_id=@uid",
                    con, tx);

                checkCmd.Parameters.Add("@uid", SqlDbType.Int).Value = dto.user_id;

                int cartCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (cartCount == 0)
                {
                    tx.Rollback();
                    return false;
                }

                // 2️⃣ Create order
                var orderCmd = new SqlCommand(@"
                    INSERT INTO cfg_set_order
                    (user_id, order_no, order_total, order_status, placed_time)
                    VALUES
                    (@uid, @ono,
                     (SELECT SUM(total_price) FROM cfg_set_cart WHERE user_id=@uid),
                     'Pending', GETDATE());

                    SELECT SCOPE_IDENTITY();", con, tx);

                orderCmd.Parameters.Add("@uid", SqlDbType.Int).Value = dto.user_id;
                orderCmd.Parameters.Add("@ono", SqlDbType.NVarChar).Value = "ORD-" + Guid.NewGuid();

                int orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());

                // 3️⃣ Move cart → order items
                var itemCmd = new SqlCommand(@"
                    INSERT INTO cfg_set_order_item
                    (user_id, order_id, medicine_id, unit_price, discount, qty, total_price)
                    SELECT user_id, @oid, medicine_id, unit_price, discount, qty, total_price
                    FROM cfg_set_cart WHERE user_id=@uid",
                    con, tx);

                itemCmd.Parameters.Add("@oid", SqlDbType.Int).Value = orderId;
                itemCmd.Parameters.Add("@uid", SqlDbType.Int).Value = dto.user_id;

                await itemCmd.ExecuteNonQueryAsync();

                // 4️⃣ Clear cart
                var clearCmd = new SqlCommand(
                    "DELETE FROM cfg_set_cart WHERE user_id=@uid",
                    con, tx);

                clearCmd.Parameters.Add("@uid", SqlDbType.Int).Value = dto.user_id;

                await clearCmd.ExecuteNonQueryAsync();

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // 🔹 USER ORDER LIST
        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = new List<Order>();

            using var con = new SqlConnection(_cs);
            await con.OpenAsync();

            // 1️⃣ Get Orders
            var orderCmd = new SqlCommand(@"
                SELECT id AS order_id, order_no, order_total, order_status
                FROM cfg_set_order
                WHERE user_id = @uid
                ORDER BY id DESC", con);

            orderCmd.Parameters.Add("@uid", SqlDbType.Int).Value = userId;

            using var orderReader = await orderCmd.ExecuteReaderAsync();

            while (await orderReader.ReadAsync())
            {
                orders.Add(new Order
                {
                    order_id = orderReader.GetInt32(orderReader.GetOrdinal("order_id")),
                    order_no = orderReader.GetString(orderReader.GetOrdinal("order_no")),
                    order_total = orderReader.GetDecimal(orderReader.GetOrdinal("order_total")),
                    order_status = orderReader.GetString(orderReader.GetOrdinal("order_status")),
                    items = new List<OrderItem>()
                });
            }

            await orderReader.CloseAsync();

            // 2️⃣ Get Items
            var itemCmd = new SqlCommand(@"
                SELECT 
                    oi.order_id,
                    oi.qty,
                    oi.unit_price,
                    oi.total_price,
                    m.medicine_name,
                    m.image_url
                FROM cfg_set_order_item oi
                JOIN cfg_set_medicine m ON m.id = oi.medicine_id
                WHERE oi.order_id IN (
                    SELECT id FROM cfg_set_order WHERE user_id = @uid
                )", con);

            itemCmd.Parameters.Add("@uid", SqlDbType.Int).Value = userId;

            using var itemReader = await itemCmd.ExecuteReaderAsync();

            while (await itemReader.ReadAsync())
            {
                var item = new OrderItem
                {
                    order_id = itemReader.GetInt32(itemReader.GetOrdinal("order_id")),
                    medicine_name = itemReader.GetString(itemReader.GetOrdinal("medicine_name")),
                    image_url = itemReader.GetString(itemReader.GetOrdinal("image_url")),
                    qty = itemReader.GetInt32(itemReader.GetOrdinal("qty")),
                    unit_price = itemReader.GetDecimal(itemReader.GetOrdinal("unit_price")),
                    total_price = itemReader.GetDecimal(itemReader.GetOrdinal("total_price"))
                };

                var order = orders.FirstOrDefault(o => o.order_id == item.order_id);
                if (order != null)
                    order.items.Add(item);
            }

            return orders;
        }

        // 🔹 ORDER DETAILS
        public async Task<Order?> GetOrderDetailsAsync(int userId, int orderId)
        {
            using var con = new SqlConnection(_cs);
            await con.OpenAsync();

            var cmd = new SqlCommand(
                "SELECT * FROM cfg_set_order WHERE id=@oid AND user_id=@uid", con);

            cmd.Parameters.Add("@oid", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@uid", SqlDbType.Int).Value = userId;

            using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync()) return null;

            var order = new Order
            {
                order_id = r.GetInt32(r.GetOrdinal("id")),
                order_no = r.GetString(r.GetOrdinal("order_no")),
                order_total = r.GetDecimal(r.GetOrdinal("order_total")),
                order_status = r.GetString(r.GetOrdinal("order_status")),
                items = new List<OrderItem>()
            };

            await r.CloseAsync();

            var itemCmd = new SqlCommand(@"
                SELECT oi.*, m.medicine_name, m.image_url
                FROM cfg_set_order_item oi
                JOIN cfg_set_medicine m ON m.id = oi.medicine_id
                WHERE oi.order_id=@oid", con);

            itemCmd.Parameters.Add("@oid", SqlDbType.Int).Value = orderId;

            using var ir = await itemCmd.ExecuteReaderAsync();

            while (await ir.ReadAsync())
            {
                order.items.Add(new OrderItem
                {
                    medicine_name = ir.GetString(ir.GetOrdinal("medicine_name")),
                    image_url = ir.GetString(ir.GetOrdinal("image_url")),
                    qty = ir.GetInt32(ir.GetOrdinal("qty")),
                    unit_price = ir.GetDecimal(ir.GetOrdinal("unit_price")),
                    total_price = ir.GetDecimal(ir.GetOrdinal("total_price"))
                });
            }

            return order;
        }

        // 🔹 CANCEL ORDER (No function → direct update)
        public async Task<int> CancelOrderAsync(CancelOrderDto dto)
        {
            using var con = new SqlConnection(_cs);

            var cmd = new SqlCommand(@"
                UPDATE cfg_set_order
                SET order_status = 'Cancelled',
                    cancel_reason = 'User Cancelled'
                WHERE id = @oid AND user_id = @uid", con);

            cmd.Parameters.Add("@oid", SqlDbType.Int).Value = dto.order_id;
            cmd.Parameters.Add("@uid", SqlDbType.Int).Value = dto.user_id;

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}