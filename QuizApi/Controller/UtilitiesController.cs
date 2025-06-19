// Controllers/UtilitiesController.cs dosyasının tam hali
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq; // XDocument kullanmak için

namespace QuizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        // HttpClient'ı verimli kullanmak için IHttpClientFactory'i enjekte ediyoruz.
        private readonly IHttpClientFactory _clientFactory;

        public UtilitiesController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET: /api/utilities/get-dictionary-entry/programming
        [HttpGet("get-dictionary-entry/{word}")]
        [Produces("application/xml")] // Yanıtı XML olarak üreteceğiz
        public async Task<IActionResult> GetDictionaryEntry(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest("Lütfen bir kelime girin.");
            }

            // IHttpClientFactory'den bir HttpClient örneği alıyoruz.
            var client = _clientFactory.CreateClient();
            
            // API'nin URL'si. XML formatında yanıt almak için accept header'ı kullanacağız.
            var requestUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";

            // Harici API'ye XML istediğimizi belirtiyoruz.
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

            try
            {
                // İsteği gönderiyoruz ve yanıtı bekliyoruz.
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                // --- HATA YÖNETİMİ (Gereksinim #7) ---
                if (response.IsSuccessStatusCode)
                {
                    // Başarılı ise (2xx durum kodları)
                    var xmlString = await response.Content.ReadAsStringAsync();
                    
                    // Gelen yanıt JSON olabilir, bunu XML'e çevirelim.
                    // Not: Bu API resmi XML desteği sunmuyor, JSON dönüyor.
                    // Biz bu JSON'u alıp basit bir XML'e çevireceğiz.
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(xmlString);
                    var root = jsonDoc.RootElement[0]; // İlk tanımı al
                    var aWord = root.GetProperty("word").GetString();
                    var definition = root.GetProperty("meanings")[0].GetProperty("definitions")[0].GetProperty("definition").GetString();

                    // Kendi basit XML'imizi oluşturuyoruz.
                    var resultXml = new XElement("DictionaryEntry",
                        new XAttribute("word", aWord),
                        new XElement("Definition", definition)
                    );

                    return Content(resultXml.ToString(), "application/xml");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Eğer kelime bulunamazsa (404)
                    return NotFound(new { Message = $"'{word}' kelimesi sözlükte bulunamadı." });
                }
                else
                {
                    // Diğer hatalar için (500, 403 vb.)
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, 
                        new { Message = "Harici sözlük servisinden bir hata alındı.", Details = errorContent });
                }
            }
            catch (HttpRequestException ex)
            {
                // Ağ hatası veya DNS sorunu gibi durumlarda
                return StatusCode(503, new { Message = "Harici sözlük servisine ulaşılamıyor.", Error = ex.Message });
            }
        }
    }
}