// Controllers/ResultsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
namespace QuizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public ResultsController(QuizDbContext context) { _context = context; }

        // POST: /api/results - Bir sınav sonucunu kaydeder
        [HttpPost]
        public async Task<IActionResult> PostResult([FromBody] ResultCreateDto resultDto)
        {
            // Gelen DTO'yu tam bir QuizResult nesnesine dönüştürüyoruz.
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var newResult = new QuizResult
            {
                Score = resultDto.Score,
                QuizId = resultDto.QuizId,
                UserId = userId,
                SubmissionDate = DateTime.UtcNow
            };

            _context.QuizResults.Add(newResult);
            await _context.SaveChangesAsync();

            // CreatedAtAction'ı şimdilik basitleştirelim, çünkü GET /api/results/{id} diye bir endpoint'imiz yok.
            return StatusCode(201, newResult);
        }

      

      [HttpGet("my-results")]
public async Task<ActionResult<IEnumerable<ResultDto>>> GetMyResults()
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    var results = await _context.QuizResults.Where(r => r.UserId == userId).OrderByDescending(r => r.SubmissionDate).ToListAsync();
    var quizIds = results.Select(r => r.QuizId).ToList();
    var quizzes = await _context.Quizzes.Where(q => quizIds.Contains(q.Id)).Include(q => q.Questions).ToDictionaryAsync(q => q.Id);
    
    var resultDtos = results.Select(r => new ResultDto {
        Id = r.Id, Score = r.Score, SubmissionDate = r.SubmissionDate,
        QuizTitle = quizzes.ContainsKey(r.QuizId) ? quizzes[r.QuizId].Title : "Silinmiş Sınav",
        TotalQuestions = quizzes.ContainsKey(r.QuizId) ? quizzes[r.QuizId].Questions.Count : 0
    }).ToList();
    
    return Ok(resultDtos);
}

[HttpGet("all")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<IEnumerable<ResultDto>>> GetAllResults()
{
    var results = await _context.QuizResults.OrderByDescending(r => r.SubmissionDate).ToListAsync();
    var userIds = results.Select(r => r.UserId).ToList();
    var quizIds = results.Select(r => r.QuizId).ToList();
    var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);
    var quizzes = await _context.Quizzes.Where(q => quizIds.Contains(q.Id)).Include(q => q.Questions).ToDictionaryAsync(q => q.Id);

    var resultDtos = results.Select(r => new ResultDto {
        Id = r.Id, Score = r.Score, SubmissionDate = r.SubmissionDate,
        Username = users.ContainsKey(r.UserId) ? users[r.UserId].Username : "Bilinmeyen Kullanıcı",
        QuizTitle = quizzes.ContainsKey(r.QuizId) ? quizzes[r.QuizId].Title : "Bilinmeyen Sınav",
        TotalQuestions = quizzes.ContainsKey(r.QuizId) ? quizzes[r.QuizId].Questions.Count : 0
    }).ToList();
    
    return Ok(resultDtos);
}


      
    }
}
