# Eğitimsel Sınav ve Test Sistemi  
### XML Tabanlı Web Servisleri ile Tasarım, Geliştirme ve Uygulama

> **Mersin Üniversitesi Bilgisayar Mühendisliği Bölümü Final Projesi**  
> Geliştirici: Sıla Durmaz 

---

## Proje Hakkında

Bu proje, **XML** teknolojilerinin (XSD, XSLT, XPath) ve **ASP.NET Core RESTful Web API** mimarisinin gerçekçi bir sınav/test platformunda nasıl bütünleşik çalışabileceğini göstermektedir.

Sistem sayesinde:
- Kullanıcılar çevrimiçi sınavlara katılabilir, sonuçlarını görebilir.
- Yöneticiler sınav ekleyebilir/düzenleyebilir.
- API ile tüm veri iletişimi **XML** formatında gerçekleşir ve XSD ile validasyon yapılır.
- Frontend, saf **HTML5/CSS3/Vanilla JS** ile modern ve responsive arayüz sunar.

---

## Kısaca Mimarisi

- **Backend:** ASP.NET Core 8, C#, Entity Framework Core, SQLite  
- **Frontend:** Vanilla JS, HTML5, CSS3  
- **Güvenlik:** JWT tabanlı kimlik doğrulama ve rol bazlı yetkilendirme  
- **Veri Formatı:** XML (XSD şemaları ile doğrulama, XSLT ile dönüşüm)
- **Test:** Swagger ile API endpoint testleri

---

## Temel Özellikler

- Kullanıcı Kayıt & Giriş (JWT destekli)
- Sınav listesi, sınav çözme, skor görüntüleme
- Yönetici paneli ile sınav ekleme/güncelleme
- XSD tabanlı XML validasyonu ve XSLT ile HTML dönüşümleri
- Rol bazlı erişim kontrolü
- Dinamik ve ayrık (decoupled) istemci-sunucu mimarisi

---

## Kurulum & Çalıştırma

1. **Projeyi Klonla:**
   ```sh
   git clone https://github.com/KULLANICI_ADIN/egitimsel-sinav-sistemi.git
   cd egitimsel-sinav-sistemi
   ## Kurulum ve Çalıştırma

1. **Projeyi Klonla**
    ```sh
    git clone https://github.com/siladurmaz/egitimsel-sinav-sistemi.git
    cd egitimsel-sinav-sistemi
    ```

2. **Backend'i Başlat**
    ```sh
    cd QuizApi
    dotnet restore
    dotnet ef database update
    dotnet run
    ```
    API varsayılan olarak [http://localhost:5041](http://localhost:5041) adresinde çalışır.

3. **Frontend'i Aç**
    ```sh
    cd QuizFrontend
    ```
    `index.html` dosyasını tarayıcıda açabilirsin.

4. **Swagger ile API Testi**
    [http://localhost:5041/swagger](http://localhost:5041/swagger) adresinden API endpoint'lerini test edebilirsin.

