using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Npgsql;
using System.Collections.Generic;
using System.Data;

namespace EMedicineBE.Models
{
    public class DAL
    {
        public Response register(RegisterRequest users, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = @"
        INSERT INTO cfg_set_user
        (first_name, last_name, password, email, fund, type, status, picture)
        VALUES
        (@first_name, @last_name, @password, @email, 0, 'Users', 'Pending', @picture);
         ";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@first_name", users.first_name);
            cmd.Parameters.AddWithValue("@last_name", users.last_name);
            cmd.Parameters.AddWithValue("@password", users.password);
            cmd.Parameters.AddWithValue("@email", users.email);
            cmd.Parameters.AddWithValue("@picture", (object?)users.picture ?? DBNull.Value);

            connection.Open();
            int rows = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = rows > 0 ? 200 : 100;
            response.StatusMessage = rows > 0
                ? "User Registered Successfully"
                : "User Registration Failed";

            return response;
        }



        public Response login(User users, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = @"
        SELECT user_id, first_name, last_name, email, type, picture
        FROM cfg_set_user
        WHERE email = @email AND password = @password;
    ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@email", users.email);
            cmd.Parameters.AddWithValue("@password", users.password);

            connection.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                response.user = new User
                {
                    user_id = reader.GetInt32(0),
                    first_name = reader.GetString(1),
                    last_name = reader.GetString(2),
                    email = reader.GetString(3),
                    type = reader.GetString(4),
                    picture = reader.IsDBNull(5) ? null : reader.GetString(5)
                };

                response.StatusCode = 200;
                response.StatusMessage = "User Is Valid";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "User Is Invalid";
            }

            connection.Close();
            return response;
        }




        public Response viewUser(User users, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = "SELECT * FROM cfg_set_user WHERE user_id = @user_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", users.user_id);

            connection.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                users.user_id = reader.GetInt32(reader.GetOrdinal("user_id"));
                users.first_name = reader.GetString(reader.GetOrdinal("first_name"));
                users.last_name = reader.GetString(reader.GetOrdinal("last_name"));
                users.password = reader.GetString(reader.GetOrdinal("password"));
                users.email = reader.GetString(reader.GetOrdinal("email"));
                users.type = reader.GetString(reader.GetOrdinal("type"));
                users.fund = reader.IsDBNull(reader.GetOrdinal("fund"))
                    ? 0
                    : reader.GetDecimal(reader.GetOrdinal("fund"));
                users.create_time = reader.GetDateTime(reader.GetOrdinal("create_time"));

                response.StatusCode = 200;
                response.StatusMessage = "User Exists";
                response.user = users;
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "User Doesn't Exist";
                response.user = null;
            }

            connection.Close();
            return response;
        }


        public Response updateProfile(UpdateProfileRequest users, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = @"
        UPDATE cfg_set_user
        SET
            first_name = @first_name,
            last_name  = @last_name,
            password   = @password,
            email      = @email
        WHERE user_id = @user_id";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@user_id", users.user_id);
            cmd.Parameters.AddWithValue("@first_name", users.first_name);
            cmd.Parameters.AddWithValue("@last_name", users.last_name);
            cmd.Parameters.AddWithValue("@password", users.password);
            cmd.Parameters.AddWithValue("@email", users.email);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = i > 0 ? 200 : 100;
            response.StatusMessage = i > 0
                ? "Profile Updated Successfully"
                : "Some error occurred while updating profile";

            return response;
        }

        public Response addToCart(Cart cart, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = @"
        INSERT INTO cfg_set_cart
        (user_id, medicine_id, unit_price, discount, qty, total_price)
        VALUES
        (@user_id, @medicine_id, @unit_price, @discount, @qty, @total_price)";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@user_id", cart.user_id);
            cmd.Parameters.AddWithValue("@medicine_id", cart.medicine_id);
            cmd.Parameters.AddWithValue("@unit_price", cart.unit_price);
            cmd.Parameters.AddWithValue("@discount", cart.discount);
            cmd.Parameters.AddWithValue("@qty", cart.qty);
            cmd.Parameters.AddWithValue("@total_price", cart.total_price);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = i > 0 ? 200 : 100;
            response.StatusMessage = i > 0
                ? "Medicine added to cart successfully"
                : "Failed to add medicine to cart";

            return response;
        }

        public Response placeOrder(PlaceOrderDto dto, NpgsqlConnection connection)
        {
            Response response = new Response();

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Check cart
                string cartCheckSql = "SELECT COUNT(*) FROM cfg_set_cart WHERE user_id=@user_id";
                using var checkCmd = new NpgsqlCommand(cartCheckSql, connection, transaction);
                checkCmd.Parameters.AddWithValue("@user_id", dto.user_id);

                int cartCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (cartCount == 0)
                {
                    response.StatusCode = 100;
                    response.StatusMessage = "Cart is empty";
                    transaction.Rollback();
                    connection.Close();
                    return response;
                }

                // 2️⃣ Create order
                string orderSql = @"
            INSERT INTO cfg_set_order
            (user_id, order_no, order_total, order_status, placed_time)
            VALUES
            (@user_id, @order_no,
             (SELECT SUM(total_price) FROM cfg_set_cart WHERE user_id=@user_id),
             'Pending', NOW())
            RETURNING id";

                using var orderCmd = new NpgsqlCommand(orderSql, connection, transaction);
                orderCmd.Parameters.AddWithValue("@user_id", dto.user_id);
                orderCmd.Parameters.AddWithValue("@order_no", "ORD-" + Guid.NewGuid());

                int orderId = Convert.ToInt32(orderCmd.ExecuteScalar());

                // 3️⃣ Move cart → order items
                string itemSql = @"
            INSERT INTO cfg_set_order_item
            (user_id, order_id, medicine_id, unit_price, discount, qty, total_price)
            SELECT user_id, @order_id, medicine_id, unit_price, discount, qty, total_price
            FROM cfg_set_cart
            WHERE user_id=@user_id";

                using var itemCmd = new NpgsqlCommand(itemSql, connection, transaction);
                itemCmd.Parameters.AddWithValue("@order_id", orderId);
                itemCmd.Parameters.AddWithValue("@user_id", dto.user_id);
                itemCmd.ExecuteNonQuery();

                // 4️⃣ Clear cart
                string clearSql = "DELETE FROM cfg_set_cart WHERE user_id=@user_id";
                using var clearCmd = new NpgsqlCommand(clearSql, connection, transaction);
                clearCmd.Parameters.AddWithValue("@user_id", dto.user_id);
                clearCmd.ExecuteNonQuery();

                transaction.Commit();

                response.StatusCode = 200;
                response.StatusMessage = "Order placed successfully";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return response;
        }



        public Response userOrderList(int userId, NpgsqlConnection connection)
        {
            Response response = new Response();
            List<Order> orders = new List<Order>();

            connection.Open();

            // 1️⃣ Orders
            string orderSql = "SELECT * FROM cfg_set_order WHERE user_id=@user_id ORDER BY id DESC";
            using var orderCmd = new NpgsqlCommand(orderSql, connection);
            orderCmd.Parameters.AddWithValue("@user_id", userId);

            using var orderReader = orderCmd.ExecuteReader();
            while (orderReader.Read())
            {
                orders.Add(new Order
                {
                    id = orderReader.GetInt32(orderReader.GetOrdinal("id")),
                    user_id = orderReader.GetInt32(orderReader.GetOrdinal("user_id")),
                    order_no = orderReader.GetString(orderReader.GetOrdinal("order_no")),
                    order_total = orderReader.GetDecimal(orderReader.GetOrdinal("order_total")),
                    order_status = orderReader.GetString(orderReader.GetOrdinal("order_status")),
                    items = new List<OrderItem>()
                });
            }
            orderReader.Close();

            // 2️⃣ Items
            string itemSql = @"
        SELECT oi.*, m.medicine_name, m.image_url
        FROM cfg_set_order_item oi
        JOIN cfg_set_medicine m ON m.id = oi.medicine_id
        WHERE oi.user_id=@user_id";

            using var itemCmd = new NpgsqlCommand(itemSql, connection);
            itemCmd.Parameters.AddWithValue("@user_id", userId);

            using var itemReader = itemCmd.ExecuteReader();
            while (itemReader.Read())
            {
                var item = new OrderItem
                {
                    id = itemReader.GetInt32(itemReader.GetOrdinal("id")),
                    order_id = itemReader.GetInt32(itemReader.GetOrdinal("order_id")),
                    medicine_name = itemReader.GetString(itemReader.GetOrdinal("medicine_name")),
                    image_url = itemReader.GetString(itemReader.GetOrdinal("image_url"))
                };

                orders.First(o => o.id == item.order_id).items.Add(item);
            }

            connection.Close();

            response.StatusCode = 200;
            response.StatusMessage = "User order list fetched";
            response.listOrders = orders;
            return response;
        }


        public Response addUpdateMedicine(Medicine medicines, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = medicines.id == 0
                ? @"INSERT INTO cfg_set_medicine
            (medicine_name, manufacturer, unit_price, discount, qty, disease, uses, exp_date, image_url, status, type)
            VALUES
            (@medicine_name, @manufacturer, @unit_price, @discount, @qty, @disease, @uses, @exp_date, @image_url, @status, @type)"
                : @"UPDATE cfg_set_medicine SET
            medicine_name=@medicine_name,
            manufacturer=@manufacturer,
            unit_price=@unit_price,
            discount=@discount,
            qty=@qty,
            disease=@disease,
            uses=@uses,
            exp_date=@exp_date,
            image_url=@image_url,
            status=@status,
            type=@type
            WHERE id=@id";

            using var cmd = new NpgsqlCommand(sql, connection);

            if (medicines.id > 0)
                cmd.Parameters.AddWithValue("@id", medicines.id);

            cmd.Parameters.AddWithValue("@medicine_name", medicines.medicine_name);
            cmd.Parameters.AddWithValue("@manufacturer", medicines.manufacturer);
            cmd.Parameters.AddWithValue("@unit_price", medicines.unit_price);
            cmd.Parameters.AddWithValue("@discount", medicines.discount);
            cmd.Parameters.AddWithValue("@qty", medicines.qty);
            cmd.Parameters.AddWithValue("@disease", medicines.disease);
            cmd.Parameters.AddWithValue("@uses", medicines.uses);
            cmd.Parameters.AddWithValue("@exp_date", medicines.exp_date);
            cmd.Parameters.AddWithValue("@image_url", medicines.image_url);
            cmd.Parameters.AddWithValue("@status", medicines.status);
            cmd.Parameters.AddWithValue("@type", medicines.type);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = i > 0 ? 200 : 100;
            response.StatusMessage = i > 0
                ? (medicines.id == 0 ? "Medicine added successfully" : "Medicine updated successfully")
                : "Failed to add/update medicine";

            return response;
        }


        public Response userList(NpgsqlConnection connection)
        {
            Response response = new Response();
            List<User> userList = new List<User>();

            string sql = "SELECT * FROM cfg_set_user ORDER BY user_id DESC";

            using var cmd = new NpgsqlCommand(sql, connection);
            connection.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                userList.Add(new User
                {
                    user_id = reader.GetInt32(reader.GetOrdinal("user_id")),
                    first_name = reader.GetString(reader.GetOrdinal("first_name")),
                    last_name = reader.GetString(reader.GetOrdinal("last_name")),
                    email = reader.GetString(reader.GetOrdinal("email")),
                    fund = reader.IsDBNull(reader.GetOrdinal("fund")) ? 0 : reader.GetDecimal(reader.GetOrdinal("fund")),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString(reader.GetOrdinal("status")),
                    create_time = reader.IsDBNull(reader.GetOrdinal("create_time"))
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("create_time"))
                });
            }

            connection.Close();

            response.StatusCode = userList.Count > 0 ? 200 : 100;
            response.StatusMessage = userList.Count > 0 ? "User list fetched successfully" : "No users found";
            response.listUsers = userList;

            return response;
        }


        public Response getMedicines(NpgsqlConnection connection)
        {
            Response response = new Response();
            List<Medicine> medicines = new List<Medicine>();

            string sql = "SELECT * FROM cfg_set_medicine ORDER BY id DESC";

            using var cmd = new NpgsqlCommand(sql, connection);
            connection.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                medicines.Add(new Medicine
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    medicine_name = reader.GetString(reader.GetOrdinal("medicine_name")),
                    manufacturer = reader.GetString(reader.GetOrdinal("manufacturer")),
                    unit_price = reader.GetDecimal(reader.GetOrdinal("unit_price")),
                    discount = reader.GetDecimal(reader.GetOrdinal("discount")),
                    qty = reader.GetInt32(reader.GetOrdinal("qty")),

                    disease = reader.IsDBNull(reader.GetOrdinal("disease"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("disease")),

                    uses = reader.IsDBNull(reader.GetOrdinal("uses"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("uses")),

                    exp_date = reader.GetDateTime(reader.GetOrdinal("exp_date")),

                    image_url = reader.IsDBNull(reader.GetOrdinal("image_url"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("image_url")),

                    status = reader.IsDBNull(reader.GetOrdinal("status"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("status"))
                });
            }

            connection.Close();

            response.StatusCode = medicines.Count > 0 ? 200 : 100;
            response.StatusMessage = medicines.Count > 0
                ? "Medicines fetched successfully"
                : "No medicines found";

            response.listMedicines = medicines;
            return response;
        }


        public Response updateProfilePicture(int userId, string pictureUrl, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = "UPDATE cfg_set_user SET picture=@picture WHERE user_id=@user_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@picture", pictureUrl);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = i > 0 ? 200 : 100;
            response.StatusMessage = i > 0
                ? "Profile picture updated successfully"
                : "Profile picture update failed";

            return response;
        }

        public Response getCartItems(int userId, NpgsqlConnection connection)
        {
            Response response = new Response();
            List<Cart> cartList = new List<Cart>();

            string sql = @"
        SELECT c.*, m.medicine_name, m.image_url
        FROM cfg_set_cart c
        JOIN cfg_set_medicine m ON m.id = c.medicine_id
        WHERE c.user_id = @user_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);

            connection.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cartList.Add(new Cart
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    user_id = reader.GetInt32(reader.GetOrdinal("user_id")),
                    medicine_id = reader.GetInt32(reader.GetOrdinal("medicine_id")),
                    qty = reader.GetInt32(reader.GetOrdinal("qty")),
                    unit_price = reader.GetDecimal(reader.GetOrdinal("unit_price")),
                    discount = reader.GetDecimal(reader.GetOrdinal("discount")),
                    total_price = reader.GetDecimal(reader.GetOrdinal("total_price")),
                    medicine_name = reader.GetString(reader.GetOrdinal("medicine_name")),
                    image_url = reader.GetString(reader.GetOrdinal("image_url"))
                });
            }

            connection.Close();

            response.StatusCode = cartList.Count > 0 ? 200 : 100;
            response.StatusMessage = cartList.Count > 0 ? "Cart items fetched" : "Cart is empty";
            response.listCarts = cartList;

            return response;
        }


        public Response removeCartItem(int cartId, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = "DELETE FROM cfg_set_cart WHERE id=@cart_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cart_id", cartId);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = i > 0 ? 200 : 100;
            response.StatusMessage = i > 0 ? "Item removed from cart" : "Failed to remove item";

            return response;
        }

        public Response updateCartQty(int cartId, int qty, NpgsqlConnection connection)
        {
            Response response = new Response();

            string sql = "UPDATE cfg_set_cart SET qty=@qty,total_price = (@qty * unit_price) - discount WHERE id=@cart_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cart_id", cartId);
            cmd.Parameters.AddWithValue("@qty", qty);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            response.StatusCode = i > 0 ? 200 : 100;
            response.StatusMessage = i > 0 ? "Cart quantity updated" : "Failed to update quantity";

            return response;
        }

        public Response GetOrderDetails(int userId, int orderId, NpgsqlConnection connection)
        {
            Response response = new Response();
            Order order = null;

            connection.Open();

            // Order header
            string orderSql = "SELECT * FROM cfg_set_order WHERE id=@order_id AND user_id=@user_id";
            using var orderCmd = new NpgsqlCommand(orderSql, connection);
            orderCmd.Parameters.AddWithValue("@order_id", orderId);
            orderCmd.Parameters.AddWithValue("@user_id", userId);

            using var reader = orderCmd.ExecuteReader();
            if (!reader.Read())
            {
                response.StatusCode = 100;
                response.StatusMessage = "Order not found";
                connection.Close();
                return response;
            }

            order = new Order
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                user_id = reader.GetInt32(reader.GetOrdinal("user_id")),
                order_no = reader.GetString(reader.GetOrdinal("order_no")),
                order_total = reader.GetDecimal(reader.GetOrdinal("order_total")),
                order_status = reader.GetString(reader.GetOrdinal("order_status")),
                items = new List<OrderItem>()
            };
            reader.Close();

            // Items
            string itemSql = @"
        SELECT oi.*, m.medicine_name, m.image_url
        FROM cfg_set_order_item oi
        JOIN cfg_set_medicine m ON m.id = oi.medicine_id
        WHERE oi.order_id=@order_id";

            using var itemCmd = new NpgsqlCommand(itemSql, connection);
            itemCmd.Parameters.AddWithValue("@order_id", orderId);

            using var itemReader = itemCmd.ExecuteReader();
            while (itemReader.Read())
            {
                order.items.Add(new OrderItem
                {
                    id = itemReader.GetInt32(itemReader.GetOrdinal("id")),
                    medicine_name = itemReader.GetString(itemReader.GetOrdinal("medicine_name")),
                    image_url = itemReader.GetString(itemReader.GetOrdinal("image_url")),
                    qty = itemReader.GetInt32(itemReader.GetOrdinal("qty")),
                    unit_price = itemReader.GetDecimal(itemReader.GetOrdinal("unit_price")),
                    total_price = itemReader.GetDecimal(itemReader.GetOrdinal("total_price"))
                });
            }

            connection.Close();

            response.StatusCode = 200;
            response.StatusMessage = "Order details fetched successfully";
            response.order = order;

            return response;
        }

public Response cancelOrder(CancelOrderDto dto, NpgsqlConnection connection)
    {
        Response response = new Response();

        string sql = "SELECT cancel_order_fn(@user_id, @order_id)";

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@user_id", dto.user_id);
        cmd.Parameters.AddWithValue("@order_id", dto.order_id);

        connection.Open();
        int result = Convert.ToInt32(cmd.ExecuteScalar());
        connection.Close();

        if (result == 1)
        {
            response.StatusCode = 200;
            response.StatusMessage = "Order cancelled successfully";
        }
        else if (result == 0)
        {
            response.StatusCode = 100;
            response.StatusMessage = "Order not found";
        }
        else if (result == -2)
        {
            response.StatusCode = 100;
            response.StatusMessage = "Delivered order cannot be cancelled";
        }
        else
        {
            response.StatusCode = 500;
            response.StatusMessage = "Failed to cancel order";
        }

        return response;
    }
        public DataSet getInvoiceData(int userId, int orderId, NpgsqlConnection connection)
        {
            DataSet ds = new DataSet();

            // ---------- HEADER ----------
            string headerSql = "SELECT * FROM invoice_header_fn(@user_id, @order_id)";
            using (var headerCmd = new NpgsqlCommand(headerSql, connection))
            {
                headerCmd.Parameters.AddWithValue("@user_id", userId);
                headerCmd.Parameters.AddWithValue("@order_id", orderId);

                using var da = new NpgsqlDataAdapter(headerCmd);
                da.Fill(ds, "InvoiceHeader");
            }

            // ---------- ITEMS ----------
            string itemsSql = "SELECT * FROM invoice_items_fn(@user_id, @order_id)";
            using (var itemsCmd = new NpgsqlCommand(itemsSql, connection))
            {
                itemsCmd.Parameters.AddWithValue("@user_id", userId);
                itemsCmd.Parameters.AddWithValue("@order_id", orderId);

                using var da = new NpgsqlDataAdapter(itemsCmd);
                da.Fill(ds, "InvoiceItems");
            }

            return ds;
        }

    }
}
