using Azure;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMedicineBE.Models
{
    public class DAL
    {
        public Response register(RegisterRequest users, SqlConnection connection)
        {
            Response response = new Response();
            SqlCommand cmd = new SqlCommand("user_register_sp", connection);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@first_name", users.first_name);
            cmd.Parameters.AddWithValue("@last_name", users.last_name);
            cmd.Parameters.AddWithValue("@password", users.password);
            cmd.Parameters.AddWithValue("@email", users.email);
            cmd.Parameters.AddWithValue("@fund", 0);
            cmd.Parameters.AddWithValue("@type", "Users");
            cmd.Parameters.AddWithValue("@status", "Pending");
            cmd.Parameters.AddWithValue("@picture", users.picture ?? (object)DBNull.Value);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();
            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "User Registered Successfully";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "User Registration Failed";
            }

            return response;
        }

        public Response login(User users, SqlConnection connection)
        {
            Response response = new Response();

            try
            {
                if (users == null)
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Invalid request";
                    return response;
                }

                string email = users.email?.Trim();
                string password = users.password?.Trim();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Email and Password are required";
                    return response;
                }

                using (SqlCommand cmd = new SqlCommand("user_login_sp", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = email;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar, 100).Value = password;

                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User loggedUser = new User
                            {
                                user_id = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                first_name = reader["first_name"]?.ToString(),
                                last_name = reader["last_name"]?.ToString(),
                                email = reader["email"]?.ToString(),
                                type = reader["type"]?.ToString(),
                                picture = reader["picture"]?.ToString(),
                            };

                            response.StatusCode = 200;
                            response.StatusMessage = "User Is Valid";
                            response.user = loggedUser;
                        }
                        else
                        {
                            response.StatusCode = 100;
                            response.StatusMessage = "User Is Invalid";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return response;
        }



        public Response viewUser(User users, SqlConnection connection)
        {
            SqlDataAdapter da = new SqlDataAdapter("user_view_sp", connection);
            da.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;
            da.SelectCommand.Parameters.AddWithValue("@user_id", users.user_id);
            DataTable dt = new DataTable();
            da.Fill(dt);
            Response response = new Response();
            User user = new User();
            if (dt.Rows.Count > 0)
            {
                users.user_id = Convert.ToInt32(dt.Rows[0]["user_id"]);
                users.first_name = Convert.ToString(dt.Rows[0]["first_name"]);
                users.last_name = Convert.ToString(dt.Rows[0]["last_name"]);
                users.password = Convert.ToString(dt.Rows[0]["password"]);
                users.email = Convert.ToString(dt.Rows[0]["email"]);
                users.type = Convert.ToString(dt.Rows[0]["type"]);
                users.fund = Convert.ToDecimal(dt.Rows[0]["fund"]);
                users.create_time = Convert.ToDateTime(dt.Rows[0]["create_time"]);
                response.StatusCode = 200;
                response.StatusMessage = "User Exists";
                response.user = users;
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "User Doesn't Exists";
                response.user = users;
            }
            return response;
        }

        public Response updateProfile(UpdateProfileRequest users, SqlConnection connection)
        {
            Response response = new Response();
            SqlCommand cmd = new SqlCommand("update_profile_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@user_id", users.user_id);
            cmd.Parameters.AddWithValue("@first_name",users.first_name);
            cmd.Parameters.AddWithValue("@last_name", users.last_name);
            cmd.Parameters.AddWithValue("@password", users.password);
            cmd.Parameters.AddWithValue("@email", users.email);
            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();
            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "Profile Updated Succesfully";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Some error occured while updating profile";
            }

            return response;
        }

        public Response addToCart(Cart cart, SqlConnection connection)
        {
            Response response = new Response();

            SqlCommand cmd = new SqlCommand("add_to_cart_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@user_id", cart.user_id);
            cmd.Parameters.AddWithValue("@medicine_id", cart.medicine_id);
            cmd.Parameters.AddWithValue("@unit_price", cart.unit_price);
            cmd.Parameters.AddWithValue("@discount", cart.discount);
            cmd.Parameters.AddWithValue("@qty", cart.qty);
            cmd.Parameters.AddWithValue("@total_price", cart.total_price);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "Medicine added to cart successfully";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Failed to add medicine to cart";
            }

            return response;
        }

        public Response placeOrder(PlaceOrderDto dto, SqlConnection connection)
        {
            Response response = new Response();

            SqlCommand cmd = new SqlCommand("place_order_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@user_id", dto.user_id);

            SqlParameter resultParam = new SqlParameter("@result", System.Data.SqlDbType.Int);
            resultParam.Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.Add(resultParam);

            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();

            int result = Convert.ToInt32(resultParam.Value);

            if (result == 1)
            {
                response.StatusCode = 200;
                response.StatusMessage = "Order placed successfully";
            }
            else if (result == 0)
            {
                response.StatusCode = 100;
                response.StatusMessage = "Cart is empty";
            }
            else
            {
                response.StatusCode = 500;
                response.StatusMessage = "Failed to place order";
            }

            return response;
        }


        public Response userOrderList(int userId, SqlConnection connection)
        {
            Response response = new Response();

            try
            {
                SqlCommand cmd = new SqlCommand("user_order_list_sp", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@user_id", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                // ds.Tables[0] = Orders
                // ds.Tables[1] = Order Items

                if (ds.Tables.Count < 2)
                {
                    response.StatusCode = 100;
                    response.StatusMessage = "No orders found";
                    response.listOrders = new List<Order>();
                    return response;
                }

                DataTable dtOrders = ds.Tables[0];
                DataTable dtItems = ds.Tables[1];

                List<Order> orders = new List<Order>();

                // ✅ Read Orders
                foreach (DataRow row in dtOrders.Rows)
                {
                    Order order = new Order();
                    order.id = Convert.ToInt32(row["id"]);
                    order.user_id = Convert.ToInt32(row["user_id"]);
                    order.order_no = row["order_no"].ToString();
                    order.order_total = Convert.ToDecimal(row["order_total"]);
                    order.order_status = row["order_status"].ToString();
                    order.placed_time = row["placed_time"] == DBNull.Value ? null : Convert.ToDateTime(row["placed_time"]);
                    order.shipped_time = row["shipped_time"] == DBNull.Value ? null : Convert.ToDateTime(row["shipped_time"]);
                    order.out_for_delivery_time = row["out_for_delivery_time"] == DBNull.Value ? null : Convert.ToDateTime(row["out_for_delivery_time"]);
                    order.delivered_time = row["delivered_time"] == DBNull.Value ? null : Convert.ToDateTime(row["delivered_time"]);

                    order.expected_delivery_date = row["expected_delivery_date"] == DBNull.Value ? null : Convert.ToDateTime(row["expected_delivery_date"]);

                    orders.Add(order);
                }

                // ✅ Read Items and attach to orders
                foreach (DataRow row in dtItems.Rows)
                {
                    OrderItem item = new OrderItem();
                    item.id = Convert.ToInt32(row["id"]);
                    item.order_id = Convert.ToInt32(row["order_id"]);
                    item.user_id = Convert.ToInt32(row["user_id"]);
                    item.medicine_id = Convert.ToInt32(row["medicine_id"]);
                    item.unit_price = Convert.ToDecimal(row["unit_price"]);
                    item.discount = Convert.ToDecimal(row["discount"]);
                    item.qty = Convert.ToInt32(row["qty"]);
                    item.total_price = Convert.ToDecimal(row["total_price"]);
                    item.medicine_name = row["medicine_name"].ToString();
                    item.image_url = row["image_url"].ToString();

                    // find order and add item
                    var order = orders.FirstOrDefault(o => o.id == item.order_id);
                    if (order != null)
                    {
                        order.items.Add(item);
                    }
                }

                response.StatusCode = 200;
                response.StatusMessage = "User order list fetched successfully";
                response.listOrders = orders;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
                response.listOrders = new List<Order>();
            }

            return response;
        }


        public Response addUpdateMedicine(Medicine medicines, SqlConnection connection)
        {
            Response response = new Response();

            SqlCommand cmd = new SqlCommand("add_updated_medicines_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            // Add all medicine fields
            cmd.Parameters.AddWithValue("@medicine_id", medicines.id); // 0 for add
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

            if (i > 0)
            {
                // Decide message based on id (if 0 → add, else → update)
                response.StatusCode = 200;
                response.StatusMessage = medicines.id == 0
                    ? "Medicine added successfully"
                    : "Medicine updated successfully";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Failed to add/update medicine";
            }

            return response;
        }

        public Response userList(SqlConnection connection)
        {
            Response response = new Response();
            List<User> userList = new List<User>();

            SqlCommand cmd = new SqlCommand("user_list_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    User user = new User();

                    user.user_id = Convert.ToInt32(row["user_id"]);
                    user.first_name = row["first_name"].ToString();
                    user.last_name = row["last_name"].ToString();
                    user.password = row["password"].ToString();
                    user.email = row["email"].ToString();
                    user.fund = row["fund"] == DBNull.Value ? 0 : Convert.ToDecimal(row["fund"]);
                    user.status = row["status"] == DBNull.Value ? "" : row["status"].ToString();
                    user.create_time = row["create_time"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["create_time"]);

                    userList.Add(user);
                }

                response.StatusCode = 200;
                response.StatusMessage = "User list fetched successfully";
                response.listUsers = userList; // Assuming Response has listUsers property
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "No users found";
                response.listUsers = new List<User>();
            }

            return response;
        }

        public Response getMedicines(SqlConnection connection)
        {
            Response response = new Response();
            List<Medicine> medicines = new List<Medicine>();

            SqlCommand cmd = new SqlCommand("get_all_medicines_sp", connection); // Create this SP in SQL
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Medicine med = new Medicine
                    {
                        id = Convert.ToInt32(row["id"]),
                        medicine_name = row["medicine_name"].ToString(),
                        manufacturer = row["manufacturer"].ToString(),
                        unit_price = Convert.ToDecimal(row["unit_price"]),
                        discount = Convert.ToDecimal(row["discount"]),
                        qty = Convert.ToInt32(row["qty"]),
                        disease = row["disease"].ToString(),
                        uses = row["uses"].ToString(),
                        exp_date = Convert.ToDateTime(row["exp_date"]),
                        image_url = row["image_url"].ToString(),
                        status = Convert.ToInt32(row["status"])
                    };
                    medicines.Add(med);
                }

                response.StatusCode = 200;
                response.StatusMessage = "Medicines fetched successfully";
                response.listMedicines = medicines;
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "No medicines found";
                response.listMedicines = new List<Medicine>();
            }

            return response;
        }
        public Response updateProfilePicture(int userId, string pictureUrl, SqlConnection connection)
        {
            Response response = new Response();

            SqlCommand cmd = new SqlCommand("update_profile_picture_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@picture", pictureUrl);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "Profile picture updated successfully";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Profile picture update failed";
            }

            return response;
        }
        public Response getCartItems(int userId, SqlConnection connection)
        {
            Response response = new Response();
            List<Cart> cartList = new List<Cart>();

            SqlCommand cmd = new SqlCommand("get_cart_items_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@user_id", userId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Cart cart = new Cart();
                    cart.id = Convert.ToInt32(row["id"]);
                    cart.user_id = Convert.ToInt32(row["user_id"]);
                    cart.medicine_id = Convert.ToInt32(row["medicine_id"]);
                    cart.qty = Convert.ToInt32(row["qty"]);
                    cart.unit_price = Convert.ToDecimal(row["unit_price"]);
                    cart.discount = Convert.ToDecimal(row["discount"]);
                    cart.total_price = Convert.ToDecimal(row["total_price"]);

                    cart.medicine_name = row["medicine_name"].ToString();
                    cart.image_url = row["image_url"].ToString();

                    cartList.Add(cart);
                }

                response.StatusCode = 200;
                response.StatusMessage = "Cart items fetched";
                response.listCarts = cartList;
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Cart is empty";
                response.listCarts = new List<Cart>();
            }

            return response;
        }

        public Response removeCartItem(int cartId, SqlConnection connection)
        {
            Response response = new Response();

            SqlCommand cmd = new SqlCommand("remove_cart_item_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@cart_id", cartId);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "Item removed from cart";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Failed to remove item";
            }

            return response;
        }
        public Response updateCartQty(int cartId, int qty, SqlConnection connection)
        {
            Response response = new Response();

            SqlCommand cmd = new SqlCommand("update_cart_qty_sp", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@cart_id", cartId);
            cmd.Parameters.AddWithValue("@qty", qty);

            connection.Open();
            int i = cmd.ExecuteNonQuery();
            connection.Close();

            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "Cart quantity updated";
            }
            else
            {
                response.StatusCode = 100;
                response.StatusMessage = "Failed to update quantity";
            }

            return response;
        }
        public Response GetOrderDetails(int userId, int orderId, SqlConnection connection)
        {
            Response response = new Response();

            try
            {
                SqlCommand cmd = new SqlCommand("order_details_sp", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@order_id", orderId);
                cmd.Parameters.AddWithValue("@user_id", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables.Count < 2 || ds.Tables[0].Rows.Count == 0)
                {
                    response.StatusCode = 100;
                    response.StatusMessage = "Order not found";
                    response.order = null;
                    return response;
                }

                DataRow orderRow = ds.Tables[0].Rows[0];
                DataTable dtItems = ds.Tables[1];

                Order order = new Order();
                order.id = Convert.ToInt32(orderRow["id"]);
                order.user_id = Convert.ToInt32(orderRow["user_id"]);
                order.order_no = orderRow["order_no"].ToString();
                order.order_total = Convert.ToDecimal(orderRow["order_total"]);
                order.order_status = orderRow["order_status"].ToString();

                foreach (DataRow row in dtItems.Rows)
                {
                    OrderItem item = new OrderItem();
                    item.id = Convert.ToInt32(row["id"]);
                    item.order_id = Convert.ToInt32(row["order_id"]);
                    item.user_id = Convert.ToInt32(row["user_id"]);
                    item.medicine_id = Convert.ToInt32(row["medicine_id"]);
                    item.unit_price = Convert.ToDecimal(row["unit_price"]);
                    item.discount = Convert.ToDecimal(row["discount"]);
                    item.qty = Convert.ToInt32(row["qty"]);
                    item.total_price = Convert.ToDecimal(row["total_price"]);
                    item.medicine_name = row["medicine_name"].ToString();
                    item.image_url = row["image_url"].ToString();

                    order.items.Add(item);
                }

                response.StatusCode = 200;
                response.StatusMessage = "Order details fetched successfully";
                response.order = order;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
                response.order = null;
            }

            return response;
        }


    }
}
