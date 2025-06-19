// Data/QuizDbContext.cs dosyasının tam hali

using Microsoft.EntityFrameworkCore;
using QuizApi.Models; // Modellerimizi kullanabilmek için

namespace QuizApi.Data
{
    // DbContext sınıfından kalıtım alıyoruz. Bu, EF Core için temel sınıftır.
    public class QuizDbContext : DbContext
    {
        // Bu constructor, veritabanı bağlantı ayarları gibi seçenekleri
        // dışarıdan (Program.cs'den) almamızı sağlar.
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
        {
        }

        // DbSet<T> özellikleri, veritabanındaki tablolara karşılık gelir.
        // "Quizzes" adında bir tablo oluşturulacak ve bu tablo Quiz nesnelerini tutacak.
        public DbSet<Quiz> Quizzes { get; set; }

        // "Questions" adında bir tablo oluşturulacak ve bu tablo Question nesnelerini tutacak.
        public DbSet<Question> Questions { get; set; }
       
        // "Options" adında bir tablo oluşturulacak ve bu tablo Option nesnelerini tutacak.
        public DbSet<Option> Options { get; set; }
           
            public DbSet<User> Users { get; set; } // Kullanıcılar için tablo
        public DbSet<QuizResult> QuizResults { get; set; }


        // Model oluşturulurken ek yapılandırmalar yapmak için bu metodu override ediyoruz.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EF Core'un ilişkileri doğru kurmasına yardımcı oluyoruz.
            // Bir Quiz'in birden çok Question'ı olabilir.
            // Bir Question'ın da birden çok Option'ı olabilir.
            // EF Core bunu genelde otomatik anlar, ama karmaşık modellerde
            // buraya ek kurallar yazmak gerekebilir. Şimdilik bu kadarı yeterli.

            // Quiz ve Question arasındaki ilişki
            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions) // Bir Quiz'in çok sayıda Sorusu vardır
                .WithOne() // Her Sorunun bir tane Quiz'i vardır (ilişkinin diğer ucu belirtilmemiş)
                .OnDelete(DeleteBehavior.Cascade); // Quiz silinirse, ona bağlı Sorular da silinsin.

            // Question ve Option arasındaki ilişki
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options) // Bir Sorunun çok sayıda Seçeneği vardır
                .WithOne() // Her Seçeneğin bir tane Sorusu vardır
                .OnDelete(DeleteBehavior.Cascade); // Soru silinirse, ona bağlı Seçenekler de silinsin.
        }
    }
}