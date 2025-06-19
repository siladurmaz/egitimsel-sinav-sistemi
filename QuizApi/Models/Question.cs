using System.Xml.Serialization;

namespace QuizApi.Models
{
    public class Question
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlElement("Text")] // <Text> elementini bu özelliğe bağla
        public string QuestionText { get; set; }

        [XmlArray("Options")] // <Options> elementi
        [XmlArrayItem("Option")] // içindeki <Option> elementleri
        public List<Option> Options { get; set; }
    }
}