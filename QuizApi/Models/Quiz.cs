using System.Xml.Serialization;

namespace QuizApi.Models
{
    [XmlRoot("Quiz", Namespace = "http://www.quizsystem.com/quiz")]
    public class Quiz
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }
        
        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlArray("Questions")]
        [XmlArrayItem("Question")]
        public List<Question> Questions { get; set; }
    }
}