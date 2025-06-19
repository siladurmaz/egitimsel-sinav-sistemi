using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.AspNetCore.Hosting;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly QuizDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuizzesController(QuizDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- GÜNCELLENEN POST METODU ---
        // [HttpPost]
        // [Consumes("application/xml")]
        // [Produces("application/xml", "application/json")]
        // public async Task<IActionResult> CreateQuiz([FromBody] XDocument xmlDocument)
        // {
        //     if (xmlDocument == null || xmlDocument.Root == null)
        //     {
        //         return BadRequest("Gelen XML verisi boş veya geçersiz bir kök elemente sahip.");
        //     }

        //     // --- VALIDASYON BÖLÜMÜ ---
        //     var validationErrors = new List<string>();
        //     try
        //     {
        //         var schemas = new XmlSchemaSet();
        //         var schemaPath = Path.Combine(_env.WebRootPath, "schemas", "quiz.xsd");
        //         if (!System.IO.File.Exists(schemaPath))
        //         {
        //             return StatusCode(500, "Sunucu Hatası: quiz.xsd şema dosyası bulunamadı.");
        //         }

        //         schemas.Add("http://www.quizsystem.com/quiz", schemaPath);

        //         xmlDocument.Validate(schemas, (sender, e) =>
        //         {
        //             validationErrors.Add($"Satır {e.Exception.LineNumber}, Pozisyon {e.Exception.LinePosition}: {e.Message}");
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, $"Şema validasyonu sırasında beklenmedik bir sunucu hatası oluştu: {ex.ToString()}");
        //     }

        //     if (validationErrors.Any())
        //     {
        //         return BadRequest(new { Message = "XML validasyonu başarısız.", Errors = validationErrors });
        //     }
        //     // --- VALIDASYON BÖLÜMÜ SONU ---


        //     // --- DESERIALIZATION BÖLÜMÜ ---
        //     Quiz quizData;
        //     try
        //     {
        //         // ÖNEMLİ: XmlSerializer, ad alanını dikkate alır.
        //         var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Quiz), "http://www.quizsystem.com/quiz");
        //         using (var reader = xmlDocument.CreateReader())
        //         {
        //             quizData = (Quiz)serializer.Deserialize(reader);
        //         }

        //         if (quizData == null)
        //         {
        //             return BadRequest("XML, Quiz nesnesine dönüştürülemedi. Yapısal bir sorun olabilir.");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // InnerException genellikle daha faydalı bilgiler içerir.
        //         return BadRequest($"XML, Quiz nesnesine dönüştürülemedi. Hata: {ex.Message}. Detay: {ex.InnerException?.Message}");
        //     }
        //     // --- DESERIALIZATION BÖLÜMÜ SONU ---

        //     _context.Quizzes.Add(quizData);
        //     await _context.SaveChangesAsync();

        //     Console.WriteLine($"Yeni sınav '{quizData.Title}' veritabanına kaydedildi.");
        //     return CreatedAtAction(nameof(GetQuizById), new { id = quizData.Id }, quizData);
        // }
        // Controllers/QuizzesController.cs - GEÇİCİ BASİTLEŞTİRİLMİŞ CreateQuiz METODU
        [HttpPost]
        [Consumes("application/xml")]
        [Produces("application/xml", "application/json")]
        public async Task<IActionResult> CreateQuiz([FromBody] Quiz quizData)
        {
            // [FromBody] doğrudan Quiz nesnesine bağlamayı deneyecek.
            // Validasyon şimdilik yok.

            if (quizData == null)
            {
                // Eğer .NET XML'i Quiz nesnesine dönüştüremezse, burası null gelecek.
                return BadRequest("Gelen XML, Quiz modeline uygun değil veya boş. Lütfen XML yapınızı ve namespace'i kontrol edin.");
            }

            if (string.IsNullOrWhiteSpace(quizData.Title))
            {
                // Basit bir manuel kontrol
                return BadRequest("Quiz başlığı (title) boş olamaz.");
            }

            try
            {
                // Veritabanı işlemleri
                _context.Quizzes.Add(quizData);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Yeni sınav '{quizData.Title}' veritabanına kaydedildi (Basit Mod).");
                return CreatedAtAction(nameof(GetQuizById), new { id = quizData.Id }, quizData);
            }
            catch (Exception ex)
            {
                // Veritabanı hatası olursa yakala
                return StatusCode(500, $"Veritabanı kaydı sırasında bir hata oluştu: {ex.ToString()}");
            }
        }

        // --- YENİ EKLENEN GET METODLARI ---

        // 1. Tüm Sınavları Getiren Endpoint
        [HttpGet]
        [Produces("application/xml", "application/json")]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAllQuizzes()
        {
            var quizzes = await _context.Quizzes
                                        .Include(q => q.Questions)
                                        .ThenInclude(ques => ques.Options)
                                        .ToListAsync();

            return Ok(quizzes);
        }

        // 2. Belirli Bir Sınavı ID'ye Göre Getiren Endpoint
        [HttpGet("{id}")]
        [Produces("application/xml", "application/json")]
        public async Task<ActionResult<Quiz>> GetQuizById(int id)
        {
            var quiz = await _context.Quizzes
                                     .Include(q => q.Questions)
                                     .ThenInclude(ques => ques.Options)
                                     .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return Ok(quiz);
        }

        // 3. BELİRLİ BİR SINAVIN HTML DÖNÜŞÜMÜ ENDPOINT'İ
        // GET: /api/quizzes/{id}/view
        [HttpGet("{id}/view")]
        [AllowAnonymous] // Bu endpoint'e token olmadan da erişilebilsin
        [Produces("text/html")]
        public async Task<IActionResult> GetQuizAsHtml(int id)
        {
            var quiz = await _context.Quizzes
                                     .Include(q => q.Questions)
                                     .ThenInclude(ques => ques.Options)
                                     .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound("Belirtilen ID'ye sahip sınav bulunamadı.");
            }

            // 1. Quiz nesnesini XML string'ine çevir
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Quiz));
            string xmlString;
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(xmlWriter, quiz);
                    xmlString = stringWriter.ToString();
                }
            }

            // 2. XSLT dönüşümünü uygula
            var xslt = new XslCompiledTransform();
            var xsltPath = Path.Combine(_env.WebRootPath, "transforms", "quiz-to-html.xsl");
            xslt.Load(xsltPath);

            using (var stringReader = new StringReader(xmlString))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    using (var htmlWriter = new StringWriter())
                    {
                        xslt.Transform(xmlReader, null, htmlWriter);
                        // 3. Sonuç olarak HTML döndür
                        return Content(htmlWriter.ToString(), "text/html", System.Text.Encoding.UTF8);
                    }
                }
            }
        }
        // --- YENİ ENDPOINT SONU ---
        // PUT: /api/quizzes/5
        // Belirtilen ID'ye sahip sınavı günceller.
        // Controllers/QuizzesController.cs - GÜNCELLENMİŞ UpdateQuiz METODU

        // PUT: /api/quizzes/5
        // Belirtilen ID'ye sahip sınavı günceller.
        // [HttpPut("{id}")]
        // [Consumes("application/xml")]
        // public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizPutDto updatedQuizData)
        // {
        //     // Artık parametre olarak QuizPutDto alıyoruz.
        //     if (updatedQuizData == null)
        //     {
        //         return BadRequest("Gönderilen veri boş.");
        //     }

        //     // 1. Güncellenecek sınav veritabanında var mı diye kontrol et.
        //     var existingQuiz = await _context.Quizzes
        //                                     .Include(q => q.Questions)
        //                                     .ThenInclude(q => q.Options)
        //                                     .FirstOrDefaultAsync(q => q.Id == id);

        //     if (existingQuiz == null)
        //     {
        //         return NotFound("Güncellenecek sınav bulunamadı.");
        //     }

        //     // 2. Gelen DTO'daki yeni verileri, null değilse, mevcut sınav üzerine uygula.
        //     // Artık bu kontroller %100 doğru çalışacak çünkü DTO'daki alanlar nullable.
        //     if (!string.IsNullOrWhiteSpace(updatedQuizData.Title))
        //     {
        //         existingQuiz.Title = updatedQuizData.Title;
        //     }

        //     if (updatedQuizData.Description != null)
        //     {
        //         existingQuiz.Description = updatedQuizData.Description;
        //     }

        //     if (updatedQuizData.Questions != null && updatedQuizData.Questions.Any())
        //     {
        //         _context.Questions.RemoveRange(existingQuiz.Questions);
        //         existingQuiz.Questions = updatedQuizData.Questions;
        //     }

        //     try
        //     {
        //         // 3. Değişiklikleri veritabanına kaydet.
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         return NotFound("Sınav güncellenirken bir çakışma oluştu, veri artık mevcut olmayabilir.");
        //     }

        //     return NoContent();
        // }
        
        // DELETE: /api/quizzes/5
        // Belirtilen ID'ye sahip sınavı siler.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            // 1. Silinecek sınav veritabanında var mı diye bul.
            var quizToDelete = await _context.Quizzes.FindAsync(id);

            if (quizToDelete == null)
            {
                // Eğer yoksa, 404 Not Found döndür.
                return NotFound("Silinecek sınav bulunamadı.");
            }

            // 2. Sınavı veritabanı context'inden silinmek üzere işaretle.
            _context.Quizzes.Remove(quizToDelete);

            // 3. Değişikliği kalıcı hale getir.
            await _context.SaveChangesAsync();

            // DbContext'te Cascade Delete (basamaklı silme) ayarını yaptığımız için,
            // bir Quiz silindiğinde ona bağlı tüm Question'lar ve Option'lar
            // da otomatik olarak silinecektir.

            // Başarılı silme sonrası da 204 No Content döndürmek yaygın bir pratiktir.
            return NoContent();
        }
        
        [HttpPatch("{id}")]
        [Consumes("application/xml")]
        public async Task<IActionResult> PatchQuiz(int id, [FromBody] QuizPatchDto patchData)
        {
            if (patchData == null)
            {
                return BadRequest("Gönderilen yama (patch) verisi boş.");
            }

            // 1. Güncellenecek sınavı veritabanından bul.
            var quizToPatch = await _context.Quizzes.FindAsync(id);

            if (quizToPatch == null)
            {
                return NotFound("Güncellenecek sınav bulunamadı.");
            }

            // 2. Gelen veriyi uygula.
            // Sadece gelen verideki alanlar null değilse güncelleme yap.
            if (!string.IsNullOrWhiteSpace(patchData.Title))
            {
                quizToPatch.Title = patchData.Title;
            }

            if (patchData.Description != null)
            {
                // Description'ın boş bir string'e ayarlanabilmesine izin veriyoruz.
                quizToPatch.Description = patchData.Description;
            }

            // 3. Değişiklikleri kaydet.
            await _context.SaveChangesAsync();

            // Başarılı güncelleme sonrası 204 No Content döndür.
            return NoContent();
        }
    }   
}
