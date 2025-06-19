// app.js

// API'mizin temel adresi. Çalışan API'nizin portuyla eşleştiğinden emin olun.
const API_BASE_URL = "http://localhost:5041"; // <<--- KENDİ PORT NUMARANIZI BURAYA YAZIN

// Sayfa yüklendiğinde çalışacak kodlar
document.addEventListener("DOMContentLoaded", () => {
  // Formlar ve sekmeler için referansları al
  const loginForm = document.getElementById("login-form");
  const registerForm = document.getElementById("register-form");
  const loginTabBtn = document.getElementById("login-tab-btn");
  const registerTabBtn = document.getElementById("register-tab-btn");

  // Hata/Başarı mesajı elementleri
  const loginErrorEl = document.getElementById("login-error-message");
  const registerErrorEl = document.getElementById("register-error-message");
  const registerSuccessEl = document.getElementById("register-success-message");

  // Sekme değiştirme fonksiyonu
  window.showForm = (formId) => {
    // ... (bu fonksiyon aynı kalıyor, bir önceki adımdaki gibi)
    if (formId === "login-form") {
      loginForm.classList.add("active");
      registerForm.classList.remove("active");
      loginTabBtn.classList.add("active");
      registerTabBtn.classList.remove("active");
    } else {
      loginForm.classList.remove("active");
      registerForm.classList.add("active");
      loginTabBtn.classList.remove("active");
      registerTabBtn.classList.add("active");
    }
  };

  // --- YENİ BÖLÜM: KAYIT İŞLEMİ ---
  registerForm.addEventListener("submit", async (e) => {
    e.preventDefault(); // Formun sayfayı yeniden yüklemesini engelle

    // Mesajları temizle
    registerErrorEl.style.display = "none";
    registerSuccessEl.style.display = "none";

    const username = document.getElementById("register-username").value;
    const password = document.getElementById("register-password").value;

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ username, password }),
      });

      const data = await response.json();

      if (!response.ok) {
        // API'den gelen hata mesajını göster
        throw new Error(data.title || data.message || "Bir hata oluştu.");
      }

      // Başarılı kayıt mesajını göster
      registerSuccessEl.textContent =
        data.message || "Kayıt başarılı! Lütfen giriş yapın.";
      registerSuccessEl.style.display = "block";
      registerForm.reset(); // Formu temizle
      setTimeout(() => showForm("login-form"), 2000); // 2 saniye sonra giriş formuna geç
    } catch (error) {
      registerErrorEl.textContent = error.message;
      registerErrorEl.style.display = "block";
    }
  });

  // --- YENİ BÖLÜM: GİRİŞ İŞLEMİ ---
  loginForm.addEventListener("submit", async (e) => {
    e.preventDefault(); // Formun sayfayı yeniden yüklemesini engelle
    loginErrorEl.style.display = "none";

    const username = document.getElementById("login-username").value;
    const password = document.getElementById("login-password").value;

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ username, password }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.title || data.message || "Geçersiz kullanıcı adı veya şifre."
        );
      }

      // --- TOKEN'I KAYDETME ---
      // Başarılı girişte gelen token'ı tarayıcının yerel depolamasına kaydediyoruz.
      // Bu token, diğer sayfalarda API'ye yapılacak yetkili istekler için kullanılacak.
      localStorage.setItem("jwtToken", data.token);
      localStorage.setItem("username", username); // Kullanıcı adını da saklayalım

      // --- YÖNLENDİRME ---
      // Kullanıcıyı ana sayfaya yönlendir.
      window.location.href = "dashboard.html";
    } catch (error) {
      loginErrorEl.textContent = error.message;
      loginErrorEl.style.display = "block";
    }
  });

  // Başlangıçta giriş formunu göster
  showForm("login-form");
});
