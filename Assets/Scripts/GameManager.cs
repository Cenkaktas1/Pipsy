using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private bool isGamePaused = false;
    [SerializeField] private GameObject PausePanel;

    [SerializeField] private TextMeshProUGUI ScoreText;
    private int score = 0;

    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject LevelCompletePanel; // Bölüm bitirme paneli
    public bool isGameOver = false;
    public bool isLevelComplete = false; // Bölümün bitip bitmediði kontrolü

    [HideInInspector] public float CurrentObstacleSpeed = 5f; // Inspector'dan gizledik, veriyi LevelData'dan alacak
    private float MaxObstacleSpeed = 15f;
    private float IncreaseRate = 0.2f;

    [SerializeField] private TextMeshProUGUI HighestScoreText;
    private int HighestScore = 0;

    [Header("Game Over Skorlar")]
    [SerializeField] private TextMeshProUGUI endGameScoreText;
    [SerializeField] private TextMeshProUGUI FinalHighScoreText;
    [SerializeField] private TextMeshProUGUI NewRecord;
    private bool isRecordAchieved = false;

    [Header("Level Yönetimi")]
    public LevelData[] allLevels; // Oyundaki tüm levelleri buraya sürükleyeceðiz

    [Header("Tutorial Ayarlarý")]
    [SerializeField] private GameObject TutorialPanel;
    private bool isTutorialWaiting = false; // Oyuncunun dokunmasýný bekle

    [Header("Yýldýz Arayüz")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private GameObject Stars;
    public int collectedStars = 0;

    [Header("PowerUp Tutorial Ayarlarý")]
    [SerializeField] private GameObject PowerUpTutorialPanel;
    private bool isPowerUpTutorialWaiting = false;

    [Header("Slow Motion Ayarlarý")]
    public float slowMotionFactor = 0.5f;
    public float slowMotionDuration = 4f; // Tam aðýr çekimde kalma süresi
    public float transitionDuration = 1.5f; // Normal hýza yumuþak geçiþ süresi
    private bool isSlowMotionActive = false;

    [Header("SlowMotion Tutorial Ayarlarý")]
    [SerializeField] private GameObject SlowMotionTutorialPanel; // Yeni SlowMo panelimiz
    private bool isSlowMotionTutorialWaiting = false;

    [Header("Player Efekt Referanslarý")]
    public GhostEffect playerGhostEffect; // Yeni scriptimiz
    public ParticleSystem playerParticle; // Pipsy'deki mevcut kuyruk

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 120;
        PausePanel?.SetActive(false);
        GameOverPanel?.SetActive(false);
        LevelCompletePanel?.SetActive(false);
        TutorialPanel?.SetActive(false);
        PowerUpTutorialPanel?.SetActive(false);
        SlowMotionTutorialPanel?.SetActive(false);

        Time.timeScale = 1f;

        // Level verilerini LevelManager'dan (ScriptableObject) Çek
        LoadLevelData();

        if (LevelManager.currentLevel != null && LevelManager.currentLevel.levelIndex == 1 && !LevelManager.currentLevel.isEndless)
        {
            StartTutorial();
        }

        if (LevelManager.currentLevel != null && !LevelManager.currentLevel.isEndless)
        {
            ScoreText.text = LevelManager.currentLevel.targetScore.ToString();
            score = LevelManager.currentLevel.targetScore;
        }

        if(LevelManager.currentLevel != null && LevelManager.currentLevel.isEndless)
        {
            Stars.gameObject.SetActive(false);
        }
        UpdateScore();

        HighestScore = PlayerPrefs.GetInt("HighScore", 0);

        bool isMusicOn = PlayerPrefs.GetInt("MusicState", 1) == 1;
        bool isEffectOn = PlayerPrefs.GetInt("EffectState", 1) == 1;

        if (!isMusicOn)
        {
            AudioManager.instance.MusicSource.mute = true;
        }
        if (!isEffectOn)
        {
            AudioManager.instance.EffectSource.mute = true;
        }

        UpdateHighScoreUI();
    }

    private void Update()
    {
        // Eðer tutorial ekranýndaysak ve oyuncu ekrana dokunduysa
        if (isTutorialWaiting && Input.GetMouseButtonDown(0))
        {
            isTutorialWaiting = false;
            if (TutorialPanel != null) TutorialPanel.SetActive(false); // Paneli kapat

            Time.timeScale = 1f; // Zamaný normal akýþýna döndür, oyun baþlasýn!
        }

        if (isPowerUpTutorialWaiting && Input.GetMouseButtonDown(0))
        {
            isPowerUpTutorialWaiting = false;
            if (PowerUpTutorialPanel != null) PowerUpTutorialPanel.SetActive(false);

            Time.timeScale = 1f; // Oyuncu ekrana dokunursa zaman tekrar aksýn
        }

        if (isSlowMotionTutorialWaiting && Input.GetMouseButtonDown(0))
        {
            isSlowMotionTutorialWaiting = false;
            if (SlowMotionTutorialPanel != null) SlowMotionTutorialPanel.SetActive(false);

            Time.timeScale = 1f; // Oyuncu ekrana dokunursa zaman tekrar aksýn
        }
    }

    private void StartTutorial()
    {
        isTutorialWaiting = true;
        Time.timeScale = 0f; // Zamaný durdur, engeller gelmesin

        if (TutorialPanel != null)
        {
            TutorialPanel.SetActive(true);
        }
    }

    // LevelManager'dan gelen verileri deðiþkene aktar
    private void LoadLevelData()
    {
        if (LevelManager.currentLevel != null)
        {
            CurrentObstacleSpeed = LevelManager.currentLevel.baseObstacleSpeed;
            MaxObstacleSpeed = LevelManager.currentLevel.maxObstacleSpeed;
            IncreaseRate = LevelManager.currentLevel.speedIncreaseMultiplier;

            // Eðer sonsuz mod deðilse, HighScore Text'ini "Hedef: X" olarak deðiþtirebiliriz.
            if (!LevelManager.currentLevel.isEndless)
            {
                HighestScoreText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Eðer Editörde direkt bu sahneden baþlatýrsan güvenlik deðerleri
            CurrentObstacleSpeed = 5f;
            MaxObstacleSpeed = 15f;
            IncreaseRate = 0.2f;
        }
    }

    public void AddScore()
    {
        if (isGameOver || isLevelComplete) return;

        // Level Bitirme Mantýðý
        if (LevelManager.currentLevel != null && !LevelManager.currentLevel.isEndless)
        {
                score--;
                if (score < 0) score = 0;
                UpdateScore();

                if (score == 0)
                {
                    LevelComplete(); // Hedef skora ulaþýldýysa bölüm biter
                    return; // Rekor kaydetmeye gerek yok, metoddan çýk
                }
        }
        // Eðer Endless mode ise rekor mantýðý çalýþmaya devam etsin
        else
        {
            score++;
            UpdateScore();
            if (score > HighestScore)
            {
                HighestScore = score;
                isRecordAchieved = true;
                PlayerPrefs.SetInt("HighScore", HighestScore);
                UpdateHighScoreUI();
            }
        }

        if (CurrentObstacleSpeed < MaxObstacleSpeed) CurrentObstacleSpeed += IncreaseRate;
    }

    public void LevelComplete()
    {
        AudioManager.instance.PlayEffect(AudioManager.instance.LevelCompletedSound);
        isLevelComplete = true;
        Time.timeScale = 0f;

        if (LevelCompletePanel != null) LevelCompletePanel.SetActive(true);

        if (LevelManager.currentLevel != null)
        {
            string keyName = "Level_" + LevelManager.currentLevel.levelIndex + "_Stars";
            int previousStars = PlayerPrefs.GetInt(keyName, 0);

            // Eðer oyuncu eskisinden daha çok yýldýz topladýysa üstüne yaz
            if (collectedStars > previousStars)
            {
                PlayerPrefs.SetInt(keyName, collectedStars);
            }
        }

        // Sonraki levelin kilidini aç
        if (LevelManager.currentLevel != null)
        {
            int nextLevelIndex = LevelManager.currentLevel.levelIndex + 1;
            int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

            if (nextLevelIndex > reachedLevel)
            {
                PlayerPrefs.SetInt("ReachedLevel", nextLevelIndex);
                PlayerPrefs.Save();
            }
        }
    }

    private void UpdateScore()
    {
        if (ScoreText != null)
        {
            ScoreText.text = score.ToString();
        }
    }

    public void PauseGame()
    {
        if (isGameOver || isLevelComplete) return; // Bölüm bitince pause çalýþmasýn

        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            PausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            PausePanel.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (isGameOver || isLevelComplete) return;
        isGameOver = true;
        StartCoroutine(GameOverBool());
    }

    public void LoadNextLevel()
    {
        // Güvenlik kontrolü
        if (LevelManager.currentLevel == null) return;

        // Mevcut levelin listedeki sýrasýný bul
        for (int i = 0; i < allLevels.Length; i++)
        {
            if (allLevels[i] == LevelManager.currentLevel)
            {
                // Eðer bu son level deðilse, bir sonrakine geç
                if (i + 1 < allLevels.Length)
                {
                    LevelManager.currentLevel = allLevels[i + 1]; // Yeni leveli ata
                    Time.timeScale = 1f; // Zamaný tekrar baþlat
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Sahneyi yenile
                    return;
                }

                else
                {
                    // Oyuncu son leveli de bitirdiyse
                    LevelManager.currentLevel = null; // Level verisini temizle
                    Time.timeScale = 1f;

                    // Sahneyi yenile Start metodunda currentLevel null olduðu için 
                    // sistem otomatik olarak Ana Menü'yü açacaktýr.
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    return;
                }
            }
        }

        // Eðer oyuncu oyundaki EN SON leveli bitirdiyse, ana menüye gönder.
        BackToMenu();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f; // Ana menüye dönerken zamaný sýfýrlamayý unutma!
        SceneManager.LoadScene(0);
    }

    private void UpdateHighScoreUI()
    {
        if (LevelManager.currentLevel != null && !LevelManager.currentLevel.isEndless) return; // Sonsuz mod deðilse bunu atla
        HighestScoreText.text = "HI: " + HighestScore.ToString();
    }

    private IEnumerator GameOverBool()
    {
        yield return new WaitForSeconds(0.8f);

        Time.timeScale = 0f;
        GameOverPanel.SetActive(true);
        
        NewRecord.gameObject.SetActive(false);

        // GameOver durumunda rekor sadece Endless moddaysa güncellenir
        if (LevelManager.currentLevel == null || LevelManager.currentLevel.isEndless)
        {
            int bestScore = PlayerPrefs.GetInt("HighScore", 0);

            if (isRecordAchieved)
            {
                NewRecord.gameObject.SetActive(true);

                bestScore = score;
                endGameScoreText.text = "Score: " + score.ToString();
                PlayerPrefs.SetInt("HighScore", bestScore);
                PlayerPrefs.Save();
                FinalHighScoreText.text = "HI: " + bestScore.ToString();
            }
            else
            {
                FinalHighScoreText.text = "HI: " + bestScore.ToString();
                endGameScoreText.text = "Score: " + score.ToString();
            }
        }
        else
        {
            // Bölümlü modda gameover olduysa Level Baþarýsýz mesajý verir
            endGameScoreText.text = "Left: " + score.ToString();
            FinalHighScoreText.text = "Target: " + LevelManager.currentLevel.targetScore.ToString();
        }
    }

    public void AddStar()
    {
        if (collectedStars < 3)
        {
            collectedStars++;
            UpdateStarUI(); // Görselleri güncelle
        }
    }

    void UpdateStarUI()
    {
        // Diziden taþmayý önleyen güvenlik kontrolü
        int index = collectedStars - 1;

        if (index >= 0 && index < starImages.Length)
        {
            // Oyun içi yýldýzý doldur
            starImages[index].sprite = filledStarSprite;

            // Level sonu panelindeki yýldýzý doldur
            if (index + 3 < starImages.Length)
            {
                starImages[index + 3].sprite = filledStarSprite;
            }
        }
    }

    public void ShowPowerUpTutorial()
    {
        isPowerUpTutorialWaiting = true;
        Time.timeScale = 0f; // PowerUp ekrana girdiði an zamaný durdur

        if (PowerUpTutorialPanel != null)
        {
            PowerUpTutorialPanel.SetActive(true);
        }
    }

    public void ShowSlowMotionTutorial()
    {
        isSlowMotionTutorialWaiting = true;
        Time.timeScale = 0f;

        if(SlowMotionTutorialPanel != null)
        {
            SlowMotionTutorialPanel.SetActive(true);
        }
    }

    public void ActivateSlowMotion()
    {
        if(!isSlowMotionActive)
            StartCoroutine(SlowMotion());
    }

    IEnumerator SlowMotion()
    {
        isSlowMotionActive = true;

        if (playerGhostEffect != null) playerGhostEffect.StartGhosting(); // Hayaletleri aç

        if (playerParticle != null) // kuyruðu kapat
        {
            var emission = playerParticle.emission;
            emission.enabled = false;
        }

        Time.timeScale = slowMotionFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Titreme olmamasý için oyun motorunu da belirli ölçüde yavaþlatýr.

        yield return new WaitForSecondsRealtime(slowMotionDuration); // Gerçek zamanlý olarak aðýr çekimde bekle

        float elapsedTime = 0f;
        while (elapsedTime < slowMotionDuration)
        {
            // Zaman yavaþken Time.deltaTime hatalý sonuç vereceði için unscaledDeltaTime kullanýyoruz
            elapsedTime += Time.unscaledDeltaTime;
            // Zamaný slowMoFactor(0.5) ile 1.0 arasýnda yumuþakça artýr
            float currentScale = Mathf.Lerp(slowMotionFactor, 1f, elapsedTime / transitionDuration);

            Time.timeScale = currentScale;
            Time.fixedDeltaTime = 0.02f * currentScale;

            yield return null; // Bir sonraki Frame'i bekle
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (playerGhostEffect != null) playerGhostEffect.StopGhosting(); // Hayaletleri durdur

        if (playerParticle != null) // Kuyruðu geri aç
        {
            var emission = playerParticle.emission;
            emission.enabled = true;
        }

        isSlowMotionActive = false;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}