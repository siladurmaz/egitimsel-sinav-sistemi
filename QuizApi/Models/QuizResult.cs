// Models/QuizResult.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApi.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public DateTime SubmissionDate { get; set; }

        // İlişkiler
        // [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } // Kimin sonucu
        // [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        
        [NotMapped] // Bu alanı veritabanına ekleme, sadece JSON/XML için
        public Quiz? Quiz { get; set; } // Hangi sınavın sonucu
    }
}