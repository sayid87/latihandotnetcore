using System.Data;
using MySqlConnector;

class User
{
    private readonly string _connectionString;
    public User(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public List<dynamic> ListData()
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();
        var dataUser = new List<dynamic>();
        var cmd = new MySqlCommand("SELECT * FROM users", con);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            // users.Add(new Dictionary<string, object> { id = reader.GetInt32("id_user"), email = reader.GetString("email") });
            dataUser.Add(new Dictionary<string, object>
            {
                {"id" , reader.GetInt32("id_user")},
                {"email" , reader.GetString("email")},
                {"telp" , reader.GetString("telp")},
                {"foto" , reader.GetString("foto")},
                {"nama" , reader.GetString("nama")},

            });
        }
        return dataUser;
    }

    public void TambahUser(string nama, string telp, string email, string password, string foto)
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        string sql = "INSERT INTO users (nama, telp, password, email,foto) VALUES (@nama, @telp, @password, @email,@foto)";
        Console.WriteLine(sql);
        var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@nama", nama);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@telp", telp);
        cmd.Parameters.AddWithValue("@foto", foto);
        cmd.Parameters.AddWithValue("@password", hashedPassword);

        cmd.ExecuteNonQuery();
    }

    public void UbahUser(int id, string nama, string telp, string foto)
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();
        MySqlCommand cmd = null;
        if (foto.Equals(""))
        {
            string sql = "UPDATE users SET nama=@nama, telp=@telp WHERE id_user = @id";
            cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@nama", nama);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@telp", telp);
        }
        else
        {
            string sql = "UPDATE users SET nama=@nama, telp=@telp,foto=@foto WHERE id_user = @id";
            cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@nama", nama);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@telp", telp);
            cmd.Parameters.AddWithValue("@foto", foto);
        }
        cmd.ExecuteNonQuery();
    }

    public void HapusUser(int id)
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();

        string sql = "DELETE FROM users WHERE id_user = @id";
        Console.WriteLine(sql);
        var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public Dictionary<string, object> CekEmailUser(string email)
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();
        var cmd = new MySqlCommand("SELECT * FROM users WHERE email = @email", con);
        cmd.Parameters.AddWithValue("@email", email);
        var reader = cmd.ExecuteReader();
        Dictionary<string, object> data = new Dictionary<string, object>();
        while (reader.Read())
        {
            data.Add("id", reader.GetInt32("id_user"));
            data.Add("nama", reader.GetString("nama"));
            data.Add("email", reader.GetString("email"));
            data.Add("telp", reader.GetString("telp"));
            data.Add("foto", reader.GetString("foto"));
        }
        return data;
    }

    public Dictionary<string, object> DetailUser(int id)
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();
        var cmd = new MySqlCommand("SELECT * FROM users WHERE id_user = @id", con);
        cmd.Parameters.AddWithValue("@id", id);
        var reader = cmd.ExecuteReader();
        Dictionary<string, object> data = new Dictionary<string, object>();
        while (reader.Read())
        {
            data.Add("id", reader.GetInt32("id_user"));
            data.Add("nama", reader.GetString("nama"));
            data.Add("email", reader.GetString("email"));
            data.Add("telp", reader.GetString("telp"));
            data.Add("foto", reader.GetString("foto"));
        }
        return data;
    }

    public Dictionary<string, object> LoginUser(string email, string password)
    {
        var con = new MySqlConnection(_connectionString);
        con.Open();
        var cmd = new MySqlCommand("SELECT * FROM users WHERE email = @email", con);
        cmd.Parameters.AddWithValue("@email", email);
        var reader = cmd.ExecuteReader();
        Dictionary<string, object> data = new Dictionary<string, object>();

        if (reader.Read())
        {
            // Mendapatkan hash password dari database
            string storedHashPassword = reader.GetString("password");

            // Memverifikasi password yang diberikan dengan hash yang ada di database
            if (BCrypt.Net.BCrypt.Verify(password, storedHashPassword))
            {
                // Password cocok
                data.Add("id", reader.GetInt32("id_user"));
                data.Add("nama", reader.GetString("nama"));
                data.Add("email", reader.GetString("email"));
                data.Add("telp", reader.GetString("telp"));
                data.Add("foto", reader.GetString("foto"));
                return data;
            }
            else
            {
                // Password tidak cocok
                return null;
            }
        }
        else
        {
            // Email tidak ditemukan
            return null;
        }


    }
}