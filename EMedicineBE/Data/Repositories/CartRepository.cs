using EMedicineBE.Entities;
using Npgsql;

public class CartRepository : ICartRepository
{
    private readonly string _cs;
    public CartRepository(IConfiguration cfg)
        => _cs = cfg.GetConnectionString("PostgresCS");

    public async Task<bool> AddToCart(Cart cart)
    {
        using var con = new NpgsqlConnection(_cs);
        string sql = @"INSERT INTO cfg_set_cart
        (user_id, medicine_id, unit_price, discount, qty, total_price)
        VALUES (@u,@m,@up,@d,@q,@t)";

        using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@u", cart.user_id);
        cmd.Parameters.AddWithValue("@m", cart.medicine_id);
        cmd.Parameters.AddWithValue("@up", cart.unit_price);
        cmd.Parameters.AddWithValue("@d", cart.discount);
        cmd.Parameters.AddWithValue("@q", cart.qty);
        cmd.Parameters.AddWithValue("@t", cart.total_price);

        await con.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<Cart>> GetCartItems(int userId)
    {
        var list = new List<Cart>();
        using var con = new NpgsqlConnection(_cs);

        string sql = @"SELECT c.*, m.medicine_name, m.image_url
                       FROM cfg_set_cart c
                       JOIN cfg_set_medicine m ON m.id=c.medicine_id
                       WHERE c.user_id=@u";

        using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@u", userId);

        await con.OpenAsync();
        using var r = await cmd.ExecuteReaderAsync();

        while (await r.ReadAsync())
        {
            list.Add(new Cart
            {
                id = r.GetInt32(r.GetOrdinal("id")),
                qty = r.GetInt32(r.GetOrdinal("qty")),
                total_price = r.GetDecimal(r.GetOrdinal("total_price")),
                medicine_name = r.GetString(r.GetOrdinal("medicine_name")),
                image_url = r.GetString(r.GetOrdinal("image_url"))
            });
        }

        return list;
    }

    public async Task<bool> UpdateQty(int cartId, int qty)
    {
        using var con = new NpgsqlConnection(_cs);
        string sql = @"UPDATE cfg_set_cart
                       SET qty=@q, total_price=(unit_price*@q)-discount
                       WHERE id=@id";

        using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", cartId);
        cmd.Parameters.AddWithValue("@q", qty);

        await con.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> RemoveItem(int cartId)
    {
        using var con = new NpgsqlConnection(_cs);
        using var cmd = new NpgsqlCommand(
            "DELETE FROM cfg_set_cart WHERE id=@id", con);

        cmd.Parameters.AddWithValue("@id", cartId);
        await con.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
