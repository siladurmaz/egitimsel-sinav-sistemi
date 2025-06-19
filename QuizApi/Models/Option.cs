using System.Xml.Serialization;

namespace QuizApi.Models
{
    public class Option
    {
        [XmlIgnore] // Bu alanı XML'e dahil etme, sadece veritabanı için.
        public int Id { get; set; } // Primary Key için eklendi.

        [XmlText] // Değeri elementin içeriği olarak al
        public string Text { get; set; }

        [XmlAttribute("correct")] // "correct" attribute'unu bu özelliğe bağla
        public bool IsCorrect { get; set; }
    }
}