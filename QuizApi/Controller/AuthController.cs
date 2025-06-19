// Controllers/AuthController.cs dosyasının tam hali
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuizApi.Data;
using QuizApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace QuizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly QuizDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(QuizDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // JWT ayarlarını appsettings'den okumak için
        }

        // --- KULLANICI KAYIT ENDPOINT'İ ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserLoginDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return BadRequest("Bu kullanıcı adı zaten alınmış.");
            }

            // Şifreyi hash'liyoruz. Veritabanına asla düz metin şifre kaydetmeyin!
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var user = new User
            {
                Username = userDto.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kullanıcı başarıyla oluşturuldu." });
        }

        // --- KULLANICI GİRİŞ ENDPOINT'İ ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            {
                // Kullanıcı bulunamazsa veya şifre yanlışsa, genel bir hata mesajı veriyoruz.
                // Bu, "kullanıcı adı var ama şifre yanlış" gibi ipuçları vermemek için önemlidir.
                return Unauthorized("Geçersiz kullanıcı adı veya şifre.");
            }

            // Kullanıcı doğrulandı, şimdi JWT oluşturacağız.
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            // Token'ın içinde bulunacak olan "claim"leri (kullanıcıya dair bilgiler) tanımlıyoruz.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (kullanıcı ID'si)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Benzersiz Token ID'si
                new Claim(ClaimTypes.Name, user.Username), // Kullanıcı adı
                new Claim(ClaimTypes.Role, user.Role) // --- YENİ EKLENEN SATIR ---
                // İleride buraya roller de eklenebilir: new Claim(ClaimTypes.Role, "Admin")
            };

            // appsettings.json'dan gizli anahtarı alıyoruz.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            // İmzalama kimlik bilgilerini oluşturuyoruz (anahtar ve algoritma).
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token'ın son kullanma tarihini belirliyoruz (örneğin 1 gün).
            var expires = DateTime.Now.AddDays(1);

            // Token'ı oluşturuyoruz.
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // Token'ı string formatına çevirip geri döndürüyoruz.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("create-admin")]
public async Task<IActionResult> CreateAdminUser([FromBody] AdminCreationDto adminDto)
{
    // Bu "gizli anahtar" appsettings.json dosyasından gelecek.
    // Bu sayede herkes bu endpoint'i kullanamaz.
    var secretKey = _configuration["AdminCreationKey"];

    if (string.IsNullOrEmpty(secretKey) || adminDto.SecretKey != secretKey)
    {
        return Unauthorized("Geçersiz gizli anahtar.");
    }

    if (await _context.Users.AnyAsync(u => u.Username == adminDto.Username))
    {
        return BadRequest("Bu kullanıcı adı zaten alınmış.");
    }

    string passwordHash = BCrypt.Net.BCrypt.HashPassword(adminDto.Password);

    var adminUser = new User
    {
        Username = adminDto.Username,
        PasswordHash = passwordHash,
        Role = "Admin" // <-- ROLÜ DOĞRUDAN "Admin" OLARAK AYARLIYORUZ
    };

    _context.Users.Add(adminUser);
    await _context.SaveChangesAsync();

    return Ok(new { Message = $"'{adminUser.Username}' adlı admin kullanıcısı başarıyla oluşturuldu." });
}

// Bu endpoint için özel bir DTO
public class AdminCreationDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string SecretKey { get; set; }
}
    }
    
    // Bu bir DTO (Data Transfer Object). Sadece kullanıcıdan veri almak için kullanılır.
    // Veritabanı modelini (User.cs) doğrudan API'ye açmak iyi bir pratik değildir.
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}