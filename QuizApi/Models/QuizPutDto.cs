// // Models/QuizPutDto.cs
// using System.Xml.Serialization;

// namespace QuizApi.Models
// {
//     // Bu DTO, PUT isteğiyle gelen verileri yakalamak için kullanılır.
//     // Soru işaretleri (?), bu alanların "nullable" yani boş/eksik olabileceğini belirtir.
//     [XmlRoot("Quiz")] 
//     public class QuizPutDto
//     {
//         [XmlAttribute("title")]
//         public string? Title { get; set; } // Title eksik olabilir

//         [XmlElement("Description")]
//         public string? Description { get; set; } // Description eksik olabilir

//         [XmlArray("Questions")]
//         [XmlArrayItem("Question")]
//         public List<Question>? Questions { get; set; } // Sorular tamamen eksik olabilir
//     }
// }