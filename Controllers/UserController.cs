using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("/users")]
public class UserController : ControllerBase
{
    private readonly User _userModel;
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _userModel = new User(configuration);
        _configuration = configuration;
    }

    [Authorize]
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

    [Authorize]
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

    [Authorize]
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

    [Authorize]
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
            // Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, hasil["email"].ToString()),
                new Claim("id", hasil["id"].ToString())
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            response["sukses"] = 1;
            response["pesan"] = "Login berhasil";
            response["data"] = hasil;
            response["token"] = $"Bearer {tokenString}";
            return Ok(response);
        }

    }
}