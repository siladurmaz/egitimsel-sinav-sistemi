// Models/QuizPatchDto.cs
using System.Xml.Serialization;

namespace QuizApi.Models
{
    [XmlRoot("QuizPatch")]
    public class QuizPatchDto
    {
        // Eğer XML'de bu element varsa, değeri okunur. Yoksa, null kalır.
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
    }
}