using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/users")]
public class UserController : ControllerBase
{
    private readonly User _userModel;

    public UserController(IConfiguration configuration)
    {
        _userModel = new User(configuration);
    }

    [HttpGet]
    public IActionResult ListUser()
    {
        var response = new Dictionary<string, object>();
        var dataUser = _userModel.ListData();

        response["sukses"] = 1;
        response["pesan"] = "List User berhasil diambil";
        response["data"] = dataUser;
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult DetailUser(int id)
    {
        var response = new Dictionary<string, object>();
        var dataUser = _userModel.DetailUser(id);

        response["sukses"] = 1;
        response["pesan"] = "Detail User berhasil diambil";
        response["data"] = dataUser;
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public IActionResult HapusUser(int id)
    {
        var response = new Dictionary<string, object>();
        _userModel.HapusUser(id);
        response["sukses"] = 1;
        response["pesan"] = "User berhasil dihapus";
        return Ok(response);
    }



    [HttpPost("register")]
    public IActionResult Register([FromForm] IFormFile? foto, [FromForm] Dictionary<string, string> data)
    {
        var response = new Dictionary<string, object>();

        string nama = data.ContainsKey("nama") ? data["nama"] : "";
        string email = data.ContainsKey("email") ? data["email"] : "";
        string telp = data.ContainsKey("telp") ? data["telp"] : "";
        string password = data.ContainsKey("password") ? data["password"] : "";

        int countError = 0;
        List<string> errMsg = [];

        if (nama == null || nama.Equals(""))
        {
            countError++;
            errMsg.Add("Nama harus diisi");
        }

        if (email == null || email.Equals("") || !Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
        {
            countError++;
            errMsg.Add("Email harus diisi dengan format yang valid");
        }
        else
        {
            if (_userModel.CekEmailUser(email).Count > 0)
            {
                countError++;
                errMsg.Add("Email sudah digunakan");
            }
        }

        if (telp == null || telp.Equals("") || telp.Length < 8 || telp.Length > 12 || !Regex.IsMatch(telp, @"^\+?\d+$"))
        {
            countError++;
            errMsg.Add("Telp harus diisi dengan angka");
        }

        if (password == null || password.Equals(""))
        {
            countError++;
            errMsg.Add("Password harus diisi");
        }

        if (foto == null || (foto.ContentType != "image/jpeg" && foto.ContentType != "image/png"))
        {
            countError++;
            errMsg.Add("Foto harus diisi antara jpg atau png");
        }


        if (countError > 0)
        {
            response["sukses"] = 0;
            response["pesan"] = errMsg;
            return BadRequest(response);
        }
        else
        {

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "users");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }


            string filePath = Path.Combine(uploadsFolder, foto.FileName);

            var stream = new FileStream(filePath, FileMode.Create);
            foto.CopyTo(stream);


            _userModel.TambahUser(data["nama"], data["telp"], data["email"], data["password"], foto.FileName);
            response["sukses"] = 1;
            response["pesan"] = "User berhasil didaftarkan";
            return Ok(response);
        }

    }

    [HttpPut("{id}")]
    public IActionResult UbahUser(int id, [FromForm] IFormFile? foto, [FromForm] Dictionary<string, string> data)
    {
        var response = new Dictionary<string, object>();

        string nama = data.ContainsKey("nama") ? data["nama"] : "";
        string telp = data.ContainsKey("telp") ? data["telp"] : "";
        string password = data.ContainsKey("password") ? data["password"] : "";

        int countError = 0;
        List<string> errMsg = [];

        if (nama == null || nama.Equals(""))
        {
            countError++;
            errMsg.Add("Nama harus diisi");
        }

        if (telp == null || telp.Equals("") || telp.Length < 8 || telp.Length > 12 || !Regex.IsMatch(telp, @"^\+?\d+$"))
        {
            countError++;
            errMsg.Add("Telp harus diisi dengan angka");
        }


        if (foto != null && (foto.ContentType != "image/jpeg" && foto.ContentType != "image/png"))
        {
            countError++;
            errMsg.Add("Foto harus diisi antara jpg atau png");
        }

        if (countError > 0)
        {
            response["sukses"] = 0;
            response["pesan"] = errMsg;
            return BadRequest(response);
        }
        else
        {
            string namaFile = "";

            if (foto != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string filePath = Path.Combine(uploadsFolder, foto.FileName);

                var stream = new FileStream(filePath, FileMode.Create);
                foto.CopyTo(stream);
                namaFile = foto.FileName;
            }

            _userModel.UbahUser(id, data["nama"], data["telp"], namaFile);
            response["sukses"] = 1;
            response["pesan"] = "User berhasil diubah";
            return Ok(response);
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromForm] Dictionary<string, string> data)
    {
        var response = new Dictionary<string, object>();

        string email = data.ContainsKey("email") ? data["email"] : "";
        string password = data.ContainsKey("password") ? data["password"] : "";
        var hasil = _userModel.LoginUser(email, password);
        if (hasil == null)
        {
            response["sukses"] = 0;
            response["pesan"] = "Email atau password salah";
            return BadRequest(response);
        }
        else
        {
            response["sukses"] = 1;
            response["pesan"] = "Login berhasil";
            response["data"] = hasil;
            return Ok(response);
        }

    }
}