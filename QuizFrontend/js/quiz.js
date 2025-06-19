// js/quiz.js (NİHAİ VE TAM VERSİYON)

const API_BASE_URL = "http://localhost:5041";

// --- Gerekli değişkenleri en dışarıda tanımla ---
let currentQuizData = null;
let currentQuestionIndex = 0;
let userAnswers = [];
let finalScore = 0; // Skoru burada saklayacağız
const token = localStorage.getItem("jwtToken");

// --- Sayfa yüklendiğinde çalışacak ana fonksiyon ---
document.addEventListener("DOMContentLoaded", () => {
  // Güvenlik ve ID kontrolleri
  if (!token) {
    window.location.href = "index.html";
    return;
  }
  const params = new URLSearchParams(window.location.search);
  const quizId = params.get("id");
  if (!quizId) {
    window.location.href = "dashboard.html";
    return;
  }

  // Olay dinleyicilerini ata
  document
    .getElementById("next-btn")
    .addEventListener("click", handleNextClick);
  // YENİ BUTONUN OLAY DİNLEYİCİSİ
  document
    .getElementById("back-to-dashboard-btn")
    .addEventListener("click", saveResultAndGoToDashboard);

  // Sınavı yükle ve başlat
  loadAndStartQuiz(quizId);
});

// --- Fonksiyonlar ---

async function loadAndStartQuiz(quizId) {
  try {
    const response = await fetch(`${API_BASE_URL}/api/quizzes/${quizId}`, {
      headers: { Authorization: `Bearer ${token}`, Accept: "application/json" },
    });
    if (!response.ok) throw new Error("Sınav verisi alınamadı.");
    currentQuizData = await response.json();

    currentQuestionIndex = 0;
    userAnswers = new Array(currentQuizData.questions.length);
    document.getElementById("quiz-title-heading").textContent =
      currentQuizData.title;
    showCurrentQuestion();
  } catch (error) {
    alert(error.message);
  }
}

function showCurrentQuestion() {
  const question = currentQuizData.questions[currentQuestionIndex];
  document.getElementById("quiz-progress").textContent = `Soru ${
    currentQuestionIndex + 1
  } / ${currentQuizData.questions.length}`;
  document.getElementById("question-text").textContent = question.questionText;

  const optionsArea = document.getElementById("options-area");
  optionsArea.innerHTML = "";
  question.options.forEach((option, index) => {
    optionsArea.innerHTML += `<div class="option-item"><input type="radio" id="opt_${index}" name="option" value="${index}"><label for="opt_${index}">${option.text}</label></div>`;
  });

  const nextBtn = document.getElementById("next-btn");
  nextBtn.textContent =
    currentQuestionIndex === currentQuizData.questions.length - 1
      ? "Sınavı Bitir"
      : "Sonraki Soru";
}

function handleNextClick() {
  const selectedOption = document.querySelector('input[name="option"]:checked');
  if (!selectedOption) {
    alert("Lütfen bir seçenek işaretleyin.");
    return;
  }
  userAnswers[currentQuestionIndex] = parseInt(selectedOption.value, 10);

  if (currentQuestionIndex < currentQuizData.questions.length - 1) {
    currentQuestionIndex++;
    showCurrentQuestion();
  } else {
    showResults();
  }
}

function showResults() {
  document.getElementById("next-btn").style.display = "none";

  // Skoru hesapla ve global değişkene ata
  finalScore = 0;
  userAnswers.forEach((answer, index) => {
    if (
      answer ===
      currentQuizData.questions[index].options.findIndex((o) => o.isCorrect)
    ) {
      finalScore++;
    }
  });

  document.getElementById("quiz-container").style.display = "none";
  document.getElementById("result-container").style.display = "block";
  document.getElementById(
    "score-text"
  ).textContent = `${finalScore} / ${currentQuizData.questions.length}`;

  const summaryArea = document.getElementById("summary-area");
  summaryArea.innerHTML = "";
  currentQuizData.questions.forEach((question, index) => {
    // ... (özet doldurma kodu aynı, değişiklik yok)
    const userAnswerIndex = userAnswers[index];
    if (userAnswerIndex === undefined) return;
    const correctOptionIndex = question.options.findIndex(
      (opt) => opt.isCorrect
    );
    const isCorrect = userAnswerIndex === correctOptionIndex;
    const summaryItem = document.createElement("div");
    summaryItem.className = `summary-item ${
      isCorrect ? "correct" : "incorrect"
    }`;
    summaryItem.innerHTML = `<p><strong>Soru ${index + 1}:</strong> ${
      question.questionText
    }</p><p><strong>Cevabınız:</strong> ${
      question.options[userAnswerIndex].text
    }</p>${
      !isCorrect
        ? `<p><strong>Doğru Cevap:</strong> ${question.options[correctOptionIndex].text}</p>`
        : ""
    }`;
    summaryArea.appendChild(summaryItem);
  });
}

// --- YENİ FONKSİYON: SONUCU KAYDET VE YÖNLENDİR ---
async function saveResultAndGoToDashboard() {
  const button = document.getElementById("back-to-dashboard-btn");
  button.disabled = true;
  button.textContent = "Kaydediliyor...";

  const resultData = {
    score: finalScore, // Global değişkenden skoru al
    quizId: currentQuizData.id,
  };

  try {
    const response = await fetch(`${API_BASE_URL}/api/results`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(resultData),
    });
    if (!response.ok) {
      throw new Error(
        "Sonuç kaydedilemedi ama ana sayfaya yönlendiriliyorsunuz."
      );
    }
  } catch (error) {
    // Hata olsa bile kullanıcıyı bilgilendir ama yine de yönlendir.
    alert(error.message);
  } finally {
    // İstek başarılı da olsa, başarısız da olsa, en sonunda ana sayfaya git.
    window.location.href = "dashboard.html";
  }
}
