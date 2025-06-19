// Program.cs dosyasının güncel ve tam hali

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models; // Swagger için bu kütüphane gerekli
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;// QuizDbContext için eklendi
using QuizApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// --- YENİ CORS POLİTİKASI ADI ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


var builder = WebApplication.CreateBuilder(args);

// Servisleri (hizmetleri) yapılandırma bölümü
// ----------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddHttpClient();
// API Controller'ları XML desteği ile ekle
builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .AddNewtonsoftJson(); 

builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));



//cors ayarları 
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://127.0.0.1:5500") // Frontend adresin
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


// --- YENİ EKLENEN SWAGGER SERVİSLERİ ---
// Swagger'ın API'niz hakkında bilgi toplayabilmesi için gereken servisi ekliyoruz.
builder.Services.AddEndpointsApiExplorer(); // Bu, minimal API endpointlerini keşfetmek için gereklidir.
builder.Services.AddSwaggerGen(options =>
{
    // Swagger UI'da görünecek olan temel API bilgilerini yapılandırıyoruz.
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Educational Quiz API",
        Description = "Sınavları ve sonuçlarını XML formatında yönetmek için bir ASP.NET Core Web API.",

    });
    // --- YENİ EKLENEN SWAGGER GÜVENLİK AYARLARI ---
    // "Authorize" düğmesini eklemek için güvenlik tanımı yapıyoruz.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Lütfen Bearer'dan sonra bir boşluk bırakarak JWT girin. Örnek: 'Bearer 12345abcdef'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Tanımladığımız güvenlik şemasını tüm endpoint'lere uygula.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    // --- SWAGGER GÜVENLİK AYARLARI SONU ---
});
// --- SWAGGER SERVİSLERİ SONU ---

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<QuizDbContext>();
        // Bekleyen migration'ları veritabanına uygula.
        // Eğer veritabanı yoksa, oluşturur ve sonra migration'ları uygular.
        context.Database.Migrate(); 
        Console.WriteLine("Veritabanı migration'ları başarıyla uygulandı.");
    }
    catch (Exception ex)
    {
        // Hata olursa logla.
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migration sırasında bir hata oluştu.");
    }
}
// Middleware (ara yazılım) yapılandırma bölümü
// ----------------------------------------------------

// Geliştirme ortamında, Swagger UI'ı etkinleştiriyoruz.
if (app.Environment.IsDevelopment())
{
    // --- YENİ EKLENEN SWAGGER MIDDLEWARE'LERİ ---
    app.UseSwagger(); // Bu, swagger.json dosyasını (API tanım dosyası) oluşturur.
    app.UseSwaggerUI(options =>
    {
        // Tarayıcıda /swagger adresine gidildiğinde hangi swagger.json dosyasının
        // kullanılacağını belirtir ve arayüz başlığını ayarlar.
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Quiz API v1");
        // Ana sayfaya (/) gidildiğinde doğrudan Swagger UI'a yönlendir.
        options.RoutePrefix = string.Empty;
    });
    // --- SWAGGER MIDDLEWARE'LERİ SONU ---
}
else
{
    // Production ortamında Developer Exception Page yerine daha güvenli bir sayfa kullan.
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // --no-https ile başladığımız için bu satırı yorumda bırakıyoruz.
app.UseCors(MyAllowSpecificOrigins); //cors ayar ekledim backend ile frontedi birbnirine bağladım 

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();