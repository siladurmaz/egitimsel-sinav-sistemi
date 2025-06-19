// Models/ResultDto.cs
namespace QuizApi.Models
{
    // Bu DTO, frontend'e gönderilecek sonuç verisini temiz bir şekilde paketler.
    public class ResultDto
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string QuizTitle { get; set; }
        public int TotalQuestions { get; set; }
        public string? Username { get; set; } // Sadece admin paneli için
    }
}