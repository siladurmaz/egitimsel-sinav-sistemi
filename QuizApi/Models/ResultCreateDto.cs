// Models/ResultCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace QuizApi.Models
{
    // Bu DTO, frontend'den yeni bir sonuç kaydı oluşturmak için gelen veriyi yakalar.
    public class ResultCreateDto
    {
        [Required]
        public int Score { get; set; }

        [Required]
        public int QuizId { get; set; }
    }
}