// js/dashboard.js (Tam ve Güncel Hali)

const API_BASE_URL = "http://localhost:5041"; // <<--- KENDİ PORT NUMARANIZI BURAYA YAZIN

document.addEventListener("DOMContentLoaded", () => {
  const token = localStorage.getItem("jwtToken");
  const username = localStorage.getItem("username");

  if (!token) {
    window.location.href = "index.html";
    return;
  }

  document.getElementById(
    "welcome-message"
  ).textContent = `Hoş Geldin, ${username}!`;
  document.getElementById("logout-btn").addEventListener("click", () => {
    localStorage.removeItem("jwtToken");
    localStorage.removeItem("username");
    window.location.href = "index.html";
  });

  const quizListContainer = document.getElementById("quiz-list-container");
  const loadingMessage = document.getElementById("loading-message");
  const modal = document.getElementById("quiz-modal");
  const newQuizBtn = document.getElementById("new-quiz-btn");
  const closeBtn = document.getElementById("modal-close-btn");
  const quizForm = document.getElementById("quiz-form");
  const questionsContainer = document.getElementById("questions-container");
  const addQuestionBtn = document.getElementById("add-question-btn");
  const modalTitle = document.getElementById("modal-title");
  const quizIdInput = document.getElementById("quiz-id-input");

  // --- SINAVLARI YÜKLEME ---
  async function loadQuizzes() {
    // ... (Bu fonksiyon aynı kalıyor, bir önceki adımdaki gibi)
    try {
      const response = await fetch(`${API_BASE_URL}/api/quizzes`, {
        headers: {
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
        },
      });
      if (!response.ok) throw new Error("Sınavlar yüklenemedi.");
      const quizzes = await response.json();
      loadingMessage.style.display = "none";
      quizListContainer.innerHTML = "";
      if (quizzes.length === 0) {
        quizListContainer.innerHTML =
          "<p>Henüz oluşturulmuş bir sınav yok.</p>";
      } else {
        quizzes.forEach((quiz) => {
          const card = document.createElement("div");
          card.className = "quiz-card";
          card.innerHTML = `
                        <div class="quiz-card-header">
                            <h3>${quiz.title}</h3>
                            <div class="quiz-card-actions">
                                <button class="edit-btn" data-id="${quiz.id}"><i class="fas fa-edit"></i></button>
                                <button class="delete-btn" data-id="${quiz.id}"><i class="fas fa-trash"></i></button>
                            </div>
                        </div>
                        <p>${quiz.description}</p>
                        <button class="btn-primary start-quiz-btn" data-id="${quiz.id}">Sınavı Başlat</button>
                    `;
          quizListContainer.appendChild(card);
        });
      }
    } catch (error) {
      loadingMessage.textContent = error.message;
    }
  }

  // --- YENİ BÖLÜM: MODAL VE FORM YÖNETİMİ ---
  function openModalForNew() {
    quizForm.reset();
    questionsContainer.innerHTML = "";
    quizIdInput.value = ""; // ID'yi temizle
    modalTitle.textContent = "Yeni Sınav Oluştur";
    addQuestionBlock(); // Başlangıç için bir soru ekle
    modal.classList.add("active");
  }

  function closeModal() {
    modal.classList.remove("active");
  }

  newQuizBtn.addEventListener("click", openModalForNew);
  closeBtn.addEventListener("click", closeModal);
  window.addEventListener("click", (e) => {
    if (e.target === modal) closeModal();
  });

  // --- YENİ BÖLÜM: DİNAMİK SORU VE SEÇENEK EKLEME ---
  let questionCounter = 0;
  function addQuestionBlock() {
    questionCounter++;
    const questionId = `q_${questionCounter}`;
    const questionBlock = document.createElement("div");
    questionBlock.className = "question-block";
    questionBlock.id = questionId;
    questionBlock.innerHTML = `
            <button type="button" class="delete-question-btn" onclick="removeElement('${questionId}')">×</button>
            <div class="input-group">
                <label>Soru Metni</label>
                <input type="text" class="question-text" required>
            </div>
            <div class="options-container">
                <!-- Seçenekler buraya eklenecek -->
            </div>
            <button type="button" class="btn-secondary add-option-btn" data-question-id="${questionId}">Seçenek Ekle</button>
        `;
    questionsContainer.appendChild(questionBlock);
    addOptionBlock(questionBlock.querySelector(".options-container"), true); // İlk seçenek doğru olsun
    addOptionBlock(questionBlock.querySelector(".options-container"));
  }

  function addOptionBlock(container, isCorrect = false) {
    const optionId = `opt_${Date.now()}_${Math.random()}`;
    const optionGroup = document.createElement("div");
    optionGroup.className = "option-group";
    optionGroup.id = optionId;
    optionGroup.innerHTML = `
            <input type="radio" name="correct_option_${
              container.parentElement.id
            }" ${isCorrect ? "checked" : ""} required>
            <input type="text" class="option-text" placeholder="Seçenek metni" required>
            <button type="button" class="delete-option-btn" onclick="removeElement('${optionId}')"><i class="fas fa-times-circle"></i></button>
        `;
    container.appendChild(optionGroup);
  }

  window.removeElement = (id) => document.getElementById(id)?.remove();
  addQuestionBtn.addEventListener("click", addQuestionBlock);

  // Dinamik olarak eklenen "Seçenek Ekle" butonları için olay dinleyicisi
  questionsContainer.addEventListener("click", (e) => {
    if (e.target && e.target.classList.contains("add-option-btn")) {
      const container = e.target.previousElementSibling;
      addOptionBlock(container);
    }
  });

  // --- YENİ BÖLÜM: FORMU XML'e ÇEVİRME VE GÖNDERME ---
  quizForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    // --- FORM VERİLERİNİ XML'E ÇEVİRME (Bu kısım aynı) ---
    const title = document.getElementById("quiz-title").value;
    const description = document.getElementById("quiz-description").value;
    let questionsXML = "";
    document.querySelectorAll(".question-block").forEach((qBlock) => {
      const questionText = qBlock.querySelector(".question-text").value;
      let optionsXML = "";
      qBlock.querySelectorAll(".option-group").forEach((optGroup) => {
        const isCorrect = optGroup.querySelector('input[type="radio"]').checked;
        const optionText = optGroup.querySelector(".option-text").value;
        optionsXML += `<Option correct="${isCorrect}">${escapeXml(
          optionText
        )}</Option>`;
      });
      questionsXML += `
            <Question type="single-choice">
                <Text><![CDATA[${questionText}]]></Text>
                <Options>${optionsXML}</Options>
            </Question>`;
    });

    // --- POST / PATCH AYRIMI BURADA BAŞLIYOR ---
    const quizId = quizIdInput.value; // Gizli input'tan ID'yi al
    let apiUrl, apiMethod, requestBody;

    if (quizId) {
      // Eğer ID varsa, bu bir GÜNCELLEME işlemidir (PATCH).
      // Sadece başlık ve açıklamayı güncelleyeceğiz.
      apiUrl = `${API_BASE_URL}/api/quizzes/${quizId}`;
      apiMethod = "PATCH";
      requestBody = `
            <QuizPatch>
                <Title>${escapeXml(title)}</Title>
                <Description>${escapeXml(description)}</Description>
            </QuizPatch>`;
      // NOT: Bu proje için, düzenleme sırasında soru ekleyip çıkarmayı desteklemiyoruz.
      // Bu, işleri basitleştirir. Raporunuzda bu bir "gelecek geliştirme" olarak belirtilebilir.
      // Eğer soruları da güncellemek isteseydik, burada PUT kullanmak daha mantıklı olurdu.
    } else {
      // Eğer ID yoksa, bu YENİ bir kayıt işlemidir (POST).
      apiUrl = `${API_BASE_URL}/api/quizzes`;
      apiMethod = "POST";
      requestBody = `
            <Quiz title="${escapeXml(
              title
            )}" xmlns="http://www.quizsystem.com/quiz">
                <Description>${escapeXml(description)}</Description>
                <Questions>${questionsXML}</Questions>
            </Quiz>`;
    }

    try {
      const response = await fetch(apiUrl, {
        method: apiMethod,
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/xml",
        },
        body: requestBody,
      });

      if (!response.ok) {
        // Hata mesajını daha iyi yakalamak için
        let errorText = `İşlem başarısız. Durum: ${response.status}`;
        try {
          const errorData = await response.json();
          errorText =
            errorData.title || errorData.message || JSON.stringify(errorData);
        } catch (jsonError) {
          // Yanıt JSON değilse, text olarak oku
          errorText = await response.text();
        }
        throw new Error(errorText);
      }

      closeModal();
      loadQuizzes(); // Listeyi yenile
    } catch (error) {
      alert(`Bir hata oluştu: ${error.message}`);
    }
  });

  // XML için özel karakterleri escape etme fonksiyonu
  function escapeXml(unsafe) {
    return unsafe.replace(/[<>&'"]/g, function (c) {
      switch (c) {
        case "<":
          return "<";
        case ">":
          return ">";
        case "&":
          return "&";
        case "'":
          return "&apos;";
        case '"':
          return '"';
      }
    });
  }
  // --- YENİ BÖLÜM: SİLME VE DÜZENLEME İŞLEMLERİ ---

  // quizListContainer, tüm kartları içerdiği için, olayları bu ana konteyner üzerinden dinlemek daha verimlidir.
  // Buna "Event Delegation" denir.
  quizListContainer.addEventListener("click", async (e) => {
    const target = e.target.closest("button"); // Tıklanan elementin bir buton olup olmadığını kontrol et
    if (!target) return;

    const quizId = target.dataset.id;

    // --- SİLME BUTONUNA TIKLANDIĞINDA ---
    if (target.classList.contains("delete-btn")) {
      // Kullanıcıdan onay al
      if (
        confirm(
          `ID'si ${quizId} olan sınavı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`
        )
      ) {
        try {
          const response = await fetch(
            `${API_BASE_URL}/api/quizzes/${quizId}`,
            {
              method: "DELETE",
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );

          if (!response.ok) {
            throw new Error("Sınav silinirken bir hata oluştu.");
          }

          // Silme başarılıysa, listeyi yenile
          loadQuizzes();
        } catch (error) {
          alert(error.message);
        }
      }
    }

    // --- DÜZENLEME BUTONUNA TIKLANDIĞINDA ---
    if (target.classList.contains("edit-btn")) {
      try {
        // 1. Düzenlenecek sınavın verilerini API'den çek
        const response = await fetch(`${API_BASE_URL}/api/quizzes/${quizId}`, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        });

        if (!response.ok) {
          throw new Error("Sınav bilgileri yüklenemedi.");
        }

        const quizData = await response.json();

        // 2. Modal'ı bu verilerle doldur
        openModalForEdit(quizData);
      } catch (error) {
        alert(error.message);
      }
    }
    if (target.classList.contains("start-quiz-btn")) {
      // Kullanıcıyı, sınavın ID'sini bir URL parametresi olarak ekleyerek quiz.html'e yönlendir.
      window.location.href = `quiz.html?id=${quizId}`;
    }
  });

  // --- YENİ FONKSİYON: DÜZENLEME İÇİN MODALI AÇMA ---
  function openModalForEdit(quiz) {
    quizForm.reset();
    questionsContainer.innerHTML = "";

    // Formun temel alanlarını doldur
    modalTitle.textContent = "Sınavı Düzenle";
    quizIdInput.value = quiz.id; // Gizli input'a ID'yi ata! Bu, kaydederken PATCH mi POST mu yapacağımızı belirleyecek.
    document.getElementById("quiz-title").value = quiz.title;
    document.getElementById("quiz-description").value = quiz.description;

    // Sınavın sorularını ve seçeneklerini forma ekle
    quiz.questions.forEach((question) => {
      questionCounter++;
      const questionId = `q_${questionCounter}`;
      const questionBlock = document.createElement("div");
      questionBlock.className = "question-block";
      questionBlock.id = questionId;
      questionBlock.innerHTML = `
            <button type="button" class="delete-question-btn" onclick="removeElement('${questionId}')">×</button>
            <div class="input-group">
                <label>Soru Metni</label>
                <input type="text" class="question-text" value="${escapeXml(
                  question.questionText
                )}" required>
            </div>
            <div class="options-container"></div>
            <button type="button" class="btn-secondary add-option-btn" data-question-id="${questionId}">Seçenek Ekle</button>
        `;

      const optionsContainer =
        questionBlock.querySelector(".options-container");
      question.options.forEach((option) => {
        const optionId = `opt_${Date.now()}_${Math.random()}`;
        const optionGroup = document.createElement("div");
        optionGroup.className = "option-group";
        optionGroup.id = optionId;
        optionGroup.innerHTML = `
                <input type="radio" name="correct_option_${questionId}" ${
          option.isCorrect ? "checked" : ""
        } required>
                <input type="text" class="option-text" value="${escapeXml(
                  option.text
                )}" placeholder="Seçenek metni" required>
                <button type="button" class="delete-option-btn" onclick="removeElement('${optionId}')"><i class="fas fa-times-circle"></i></button>
            `;
        optionsContainer.appendChild(optionGroup);
      });

      questionsContainer.appendChild(questionBlock);
    });

    modal.classList.add("active");
  }
  function parseJwt(token) {
    try {
      return JSON.parse(atob(token.split(".")[1]));
    } catch (e) {
      return null;
    }
  }
  async function loadAdminPanel() {
    try {
      const decodedToken = parseJwt(token);
      const userRole = decodedToken
        ? decodedToken[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ]
        : "User";

      if (userRole === "Admin") {
        const adminPanelContainer = document.getElementById(
          "admin-panel-container"
        );
        if (!adminPanelContainer) return;

        // Önce HTML'i konteynere ekle
        adminPanelContainer.innerHTML = `
                <div class="dashboard-header">
                    <h1>Tüm Kullanıcı Sonuçları (Admin Panel)</h1>
                </div>
                <table class="results-table">
                    <thead>
                        <tr>
                            <th>Kullanıcı</th>
                            <th>Sınav Adı</th>
                            <th>Skor</th>
                            <th>Tarih</th>
                        </tr>
                    </thead>
                    <tbody id="admin-results-tbody">
                        <tr><td colspan="4" style="text-align:center;">Sonuçlar yükleniyor...</td></tr>
                    </tbody>
                </table>`;

        // ŞİMDİ, HTML eklendiğine göre, tbody'yi GÜVENLİ bir şekilde bulabiliriz.
        const tbody = adminPanelContainer.querySelector("#admin-results-tbody");
        if (!tbody) {
          console.error("Admin paneli tbody'si bulunamadı!");
          return;
        }
        const response = await fetch(`${API_BASE_URL}/api/results/all`, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        });
        if (!response.ok) {
          tbody.innerHTML = `<tr><td colspan="4">Sonuçlar yüklenemedi.</td></tr>`;
          return;
        }

        const results = await response.json();
        tbody.innerHTML = ""; // "Yükleniyor..." mesajını temizle

        if (results.length === 0) {
          tbody.innerHTML = `<tr><td colspan="4" style="text-align:center;">Henüz kaydedilmiş bir sonuç yok.</td></tr>`;
          return;
        }

        results.forEach((result) => {
          const row = tbody.insertRow();

          row.innerHTML = `
                    <td>${result.username}</td>
                    <td>${result.quizTitle}</td>
                    <td>${result.score} / ${result.totalQuestions}</td>
                    <td>${new Date(result.submissionDate).toLocaleString(
                      "tr-TR"
                    )}</td>
                `;
        });
      }
    } catch (error) {
      console.error("Admin paneli yüklenirken hata oluştu:", error);
      // Hata durumunda kullanıcıyı bilgilendir
      const adminPanelContainer = document.getElementById(
        "admin-panel-container"
      );
      if (adminPanelContainer) {
        adminPanelContainer.innerHTML = `<p style="color:red;">Admin paneli yüklenirken bir hata oluştu.</p>`;
      }
    }
  }
  //   async function loadAdminPanel() {
  //     const decodedToken = parseJwt(token);
  //     // Rolü JWT'den alıyoruz. http://schemas.microsoft.com/ws/2008/06/identity/claims/role
  //     const userRole = decodedToken
  //       ? decodedToken[
  //           "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
  //         ]
  //       : "User";

  //     if (userRole === "Admin") {
  //       const adminPanelContainer = document.getElementById(
  //         "admin-panel-container"
  //       );
  //       if (!adminPanelContainer) return; // Eğer element yoksa hata vermesin

  //       adminPanelContainer.innerHTML = `
  //         <div class="dashboard-header">
  //             <h1>Tüm Kullanıcı Sonuçları (Admin Panel)</h1>
  //         </div>
  //         <table class="results-table">
  //             <thead><tr><th>Kullanıcı</th><th>Sınav Adı</th><th>Skor</th><th>Tarih</th></tr></thead>
  //             <tbody id="admin-results-tbody"></tbody>
  //         </table>`;
  //       document.querySelector("main.container").appendChild(adminPanel);

  //       const response = await fetch(`${API_BASE_URL}/api/results/all`, {
  //         headers: {
  //           Authorization: `Bearer ${token}`,
  //           Accept: "application/json",
  //         },
  //       });
  //       const results = await response.json();
  //       const tbody = document.getElementById("admin-results-tbody");
  //       tbody.innerHTML = ""; // Temizle
  //       results.forEach((result) => {
  //         const row = tbody.insertRow();
  //         // --- DEĞİŞİKLİK BURADA ---
  //         row.innerHTML = `
  //         <td>${result.username}</td>
  //         <td>${result.quizTitle}</td>
  //         <td>${result.score} / ${result.totalQuestions}</td>
  //         <td>${new Date(result.submissionDate).toLocaleString("tr-TR")}</td>
  //     `;
  //       });
  //     }
  //   }

  // Sayfa yüklendiğinde sınavları yükle
  loadQuizzes();
  loadAdminPanel();
});
