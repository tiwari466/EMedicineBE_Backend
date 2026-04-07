using EMedicineBE.Dto.Order;
using EMedicineBE.Entities;
using Npgsql;

namespace EMedicineBE.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _cs;

        public OrderRepository(IConfiguration configuration)
        {
            _cs = configuration.GetConnectionString("PostgresCS");
        }

        // 🔹 PLACE ORDER (transaction preserved)
        public async Task<bool> PlaceOrderAsync(PlaceOrderDto dto)
        {
            using var con = new NpgsqlConnection(_cs);
            await con.OpenAsync();
            using var tx = await con.BeginTransactionAsync();

            try
            {
                // 1️⃣ Check cart
                var checkCmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM cfg_set_cart WHERE user_id=@uid",
                    con, tx);
                checkCmd.Parameters.AddWithValue("@uid", dto.user_id);

                int cartCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (cartCount == 0)
                {
                    await tx.RollbackAsync();
                    return false;
                }

                // 2️⃣ Create order
                var orderCmd = new NpgsqlCommand(@"
                    INSERT INTO cfg_set_order
                    (user_id, order_no, order_total, order_status, placed_time)
                    VALUES
                    (@uid, @ono,
                     (SELECT SUM(total_price) FROM cfg_set_cart WHERE user_id=@uid),
                     'Pending', NOW())
                    RETURNING id", con, tx);

                orderCmd.Parameters.AddWithValue("@uid", dto.user_id);
                orderCmd.Parameters.AddWithValue("@ono", "ORD-" + Guid.NewGuid());

                int orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());

                // 3️⃣ Move cart → order items
                var itemCmd = new NpgsqlCommand(@"
                    INSERT INTO cfg_set_order_item
                    (user_id, order_id, medicine_id, unit_price, discount, qty, total_price)
                    SELECT user_id, @oid, medicine_id, unit_price, discount, qty, total_price
                    FROM cfg_set_cart WHERE user_id=@uid",
                    con, tx);

                itemCmd.Parameters.AddWithValue("@oid", orderId);
                itemCmd.Parameters.AddWithValue("@uid", dto.user_id);
                await itemCmd.ExecuteNonQueryAsync();

                // 4️⃣ Clear cart
                var clearCmd = new NpgsqlCommand(
                    "DELETE FROM cfg_set_cart WHERE user_id=@uid",
                    con, tx);
                clearCmd.Parameters.AddWithValue("@uid", dto.user_id);
                await clearCmd.ExecuteNonQueryAsync();

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // 🔹 USER ORDER LIST
        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = new List<Order>();

            using var con = new NpgsqlConnection(_cs);
            await con.OpenAsync();


            // =======================
            // 1️⃣ Get Orders (Use order_id)
            // =======================

            var orderCmd = new NpgsqlCommand(@"
        SELECT id AS order_id , order_no, order_total, order_status
        FROM cfg_set_order
        WHERE user_id = @uid
        ORDER BY order_id DESC
    ", con);

            orderCmd.Parameters.AddWithValue("@uid", userId);


            using var orderReader = await orderCmd.ExecuteReaderAsync();

            while (await orderReader.ReadAsync())
            {
                orders.Add(new Order
                {
                    order_id = orderReader.GetInt32(
                        orderReader.GetOrdinal("order_id")
                    ),

                    order_no = orderReader.GetString(
                        orderReader.GetOrdinal("order_no")
                    ),

                    order_total = orderReader.GetDecimal(
                        orderReader.GetOrdinal("order_total")
                    ),

                    order_status = orderReader.GetString(
                        orderReader.GetOrdinal("order_status")
                    ),

                    items = new List<OrderItem>()
                });
            }

            await orderReader.CloseAsync();


            // =======================
            // 2️⃣ Get Items (Join by order_id)
            // =======================

            var itemCmd = new NpgsqlCommand(@"
        SELECT 
            oi.order_id,
            oi.qty,
            oi.unit_price,
            oi.total_price,
            m.medicine_name,
            m.image_url
        FROM cfg_set_order_item oi
        JOIN cfg_set_medicine m 
            ON m.id = oi.medicine_id
        WHERE oi.order_id IN (
            SELECT id AS order_id 
            FROM cfg_set_order 
            WHERE user_id = @uid
        )
    ", con);

            itemCmd.Parameters.AddWithValue("@uid", userId);


            using var itemReader = await itemCmd.ExecuteReaderAsync();

            while (await itemReader.ReadAsync())
            {
                var item = new OrderItem
                {
                    order_id = itemReader.GetInt32(
                        itemReader.GetOrdinal("order_id")
                    ),

                    medicine_name = itemReader.GetString(
                        itemReader.GetOrdinal("medicine_name")
                    ),

                    image_url = itemReader.GetString(
                        itemReader.GetOrdinal("image_url")
                    ),

                    qty = itemReader.GetInt32(
                        itemReader.GetOrdinal("qty")
                    ),

                    unit_price = itemReader.GetDecimal(
                        itemReader.GetOrdinal("unit_price")
                    ),

                    total_price = itemReader.GetDecimal(
                        itemReader.GetOrdinal("total_price")
                    )
                };


                // Attach to correct order
                var order = orders.FirstOrDefault(
                    o => o.order_id == item.order_id
                );

                if (order != null)
                {
                    order.items.Add(item);
                }
            }

            return orders;
        }


        // 🔹 ORDER DETAILS
        public async Task<Order?> GetOrderDetailsAsync(int userId, int orderId)
        {
            using var con = new NpgsqlConnection(_cs);
            await con.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT * FROM cfg_set_order WHERE id=@oid AND user_id=@uid", con);
            cmd.Parameters.AddWithValue("@oid", orderId);
            cmd.Parameters.AddWithValue("@uid", userId);

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

            var itemCmd = new NpgsqlCommand(@"
                SELECT oi.*, m.medicine_name, m.image_url
                FROM cfg_set_order_item oi
                JOIN cfg_set_medicine m ON m.id = oi.medicine_id
                WHERE oi.order_id=@oid", con);
            itemCmd.Parameters.AddWithValue("@oid", orderId);

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

        // 🔹 CANCEL ORDER
        public async Task<int> CancelOrderAsync(CancelOrderDto dto)
        {
            using var con = new NpgsqlConnection(_cs);
            var cmd = new NpgsqlCommand(
                "SELECT cancel_order_fn(@uid,@oid)", con);
            cmd.Parameters.AddWithValue("@uid", dto.user_id);
            cmd.Parameters.AddWithValue("@oid", dto.order_id);

            await con.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }
}
