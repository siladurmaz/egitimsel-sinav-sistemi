// Models/User.cs dosyasının tam hali
using System.ComponentModel.DataAnnotations; // [Required] gibi attribute'lar için

namespace QuizApi.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Şifreyi asla düz metin olarak saklamayız!
        public string Role { get; set; } = "User"; // Varsayılan rol "User"
    }
}