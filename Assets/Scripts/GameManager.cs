using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI Level;

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

    [Header("UI animator")]
    [SerializeField] private Animator animHand;
    [SerializeField] private Animator animShift;

    [Header("Obstacle Shift Ayarlarý")]
    [SerializeField] private GameObject ObstacleShiftPanel;
    private bool isObstacleShiftWaiting = false;

    [Header("Karanlýk Mod Ayarlarý")]
    [SerializeField] private Light2D globalLight;      // Global Light 2D bileþeni
    [SerializeField] private Light2D pipsyAuraLight;  // Pipsy'nin Point Light 2D bileþeni
    private float fadeSpeed = 1.5f;
    [SerializeField] private GameObject DarkModePanel;
    private bool isDarkModeWaiting = false;
    public bool isLightTransitioning = false;
    private Coroutine lightPowerUpCoroutine;

    [Header("Glitch")]
    [SerializeField] private GameObject GlitchTutorialPanel;
    private bool isGlitchWaiting = false;

    [Header("Reklam (Revive) Ayarlarý")]
    [SerializeField] private GameObject AdPanel;
    private bool hasRevivedThisLevel = false; // Oyuncu bu bölümde hakkýný kullandý mý?

    [Header("Geçiþ Reklamý (Geri Sayým) UI")]
    [SerializeField] private GameObject InterstitialTimerPanel; // 5 saniyelik sayým paneli
    [SerializeField] private TextMeshProUGUI AdTimerText; // "Reklam Baþlýyor: 5" yazýsý
    private Coroutine adTimerCoroutine;

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

        Level.text = "Level " + LevelManager.currentLevel.levelIndex.ToString();
        Level.gameObject.SetActive(true);
        globalLight.intensity = 0f;
        Time.timeScale = 0f;

        PausePanel?.SetActive(false);
        GameOverPanel?.SetActive(false);
        LevelCompletePanel?.SetActive(false);
        TutorialPanel?.SetActive(false);
        PowerUpTutorialPanel?.SetActive(false);
        SlowMotionTutorialPanel?.SetActive(false);
        ObstacleShiftPanel?.SetActive(false);
        DarkModePanel?.SetActive(false);
        GlitchTutorialPanel?.SetActive(false);
        AdPanel?.SetActive(false);
        InterstitialTimerPanel?.SetActive(false);

        if (LevelManager.currentLevel.isLevelDark)
            isLightTransitioning = true;


        // Level verilerini LevelManager'dan (ScriptableObject) Çek
        LoadLevelData();
        StartCoroutine(LevelText());


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

        if (isObstacleShiftWaiting && Input.GetMouseButtonDown(0))
        {
            isObstacleShiftWaiting = false;
            if (ObstacleShiftPanel != null) ObstacleShiftPanel.SetActive(false);

            Time.timeScale = 1f; // Oyuncu ekrana dokunursa zaman tekrar aksýn
        }

        if(isDarkModeWaiting && Input.GetMouseButtonDown(0))
        {
            isDarkModeWaiting = false;
            if(DarkModePanel != null) DarkModePanel.SetActive(false);

            Time.timeScale = 1f;

            isLightTransitioning = true;
        }

        if (isGlitchWaiting && Input.GetMouseButtonDown(0))
        {
            isGlitchWaiting = false;
            if (GlitchTutorialPanel != null) GlitchTutorialPanel.SetActive(false);

            Time.timeScale = 1f; // Oyuncu ekrana dokunursa zaman tekrar aksýn
        }

        if (isLightTransitioning)
            TurnOnTheLights();
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

    private IEnumerator LevelText()
    {
        float fadeDuration = 2f; // Iþýðýn açýlma ve yazýnýn ekranda kalma süresi
        float timer = 0f;

        // Zaman (TimeScale) 0 olduðu için gerçek zamaný kullanarak bir döngü baþlatýyoruz
        while (timer < fadeDuration)
        {
            if (globalLight != null && !LevelManager.currentLevel.isLevelDark)
            {
                // Zamanla ýþýðý 0'dan 1'e doðru yavaþça aç
                globalLight.intensity = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            }

            timer += Time.unscaledDeltaTime; // TimeScale = 0 olsa bile gerçek saniyeyi sayar
            yield return null; // Döngünün Unity'i dondurmamasý için bir sonraki frame'i bekle
        }

        // Süre dolduðunda ýþýðýn tam 1 olduðundan emin ol (Eðer karanlýk seviye deðilse)
        if (globalLight != null && !LevelManager.currentLevel.isLevelDark)
        {
            globalLight.intensity = 1f;
        }

        // 2 saniyelik þov bitti; Level yazýsýný gizle ve zamaný normal akýþýna döndür
        Level.gameObject.SetActive(false);

        if (LevelManager.currentLevel != null && LevelManager.currentLevel.levelIndex == 1 && !LevelManager.currentLevel.isEndless)
        {
            StartTutorial();
        }

        else if (LevelManager.currentLevel != null && LevelManager.currentLevel.isFirstTimeShift)
        {
            ShowObstacleShiftPanel();
        }

        else if (LevelManager.currentLevel != null && LevelManager.currentLevel.isDarkTutorial)
        {
            ShowDarkModePanel();
        }

        else if (LevelManager.currentLevel.canSpawnPowerUps || LevelManager.currentLevel.canSpawnSlowMotion)
        {
            Spawner.instance.CallPowerUp();
        }

        else if (LevelManager.currentLevel != null && LevelManager.currentLevel.isLightAvailable)
        {
            Spawner.instance.CallLight();
        }

        else if (LevelManager.currentLevel.isGlitchAvailable)
        {
            Spawner.instance.CallGlitch();
        }

        else
        {
            Time.timeScale = 1f; // Tutorial yoksa oyunu direkt baþlat
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

        // DÝKKAT: Eskiden burada direkt LevelCompletePanel açýlýyordu, onu kaldýrdýk.

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

        // YENÝ REKLAM AKIÞI: Tebrikler paneli yerine önce geri sayým panelini aç
        if (InterstitialTimerPanel != null)
        {
            InterstitialTimerPanel.SetActive(true);
            adTimerCoroutine = StartCoroutine(AdCountdownRoutine());
        }
        else
        {
            // Eðer panel tanýmlanmamýþsa direkt bölüm sonu ekranýný göster
            ShowRealLevelCompletePanel();
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

        // Oyuncu henüz reklam izlemediyse ve panel tanýmlýysa, önce Reklam teklif et
        if (!hasRevivedThisLevel && AdPanel != null)
        {
            Time.timeScale = 0f; // Oyunu durdur
            AdPanel.SetActive(true); // "Ýzle ve Canlan" panelini aç
        }
        else
        {
            // Eðer daha önce canlandýysa veya panel yoksa, gerçek Game Over'a git
            ExecuteRealGameOver();
        }
    }

    // "Hayýr, Teþekkürler" (Decline) butonuna basýlýrsa çalýþacak fonksiyon
    public void DeclineRevive()
    {
        AdPanel.SetActive(false);
        ExecuteRealGameOver(); // Gerçekten öldür
    }

    // Asýl Game Over iþlemleri (Eski GameOverBool coroutine'ini çaðýran fonksiyon)
    private void ExecuteRealGameOver()
    {
        isGameOver = true;
        StartCoroutine(GameOverBool());
    }

    public void RevivePlayer()
    {
        // 1. Paneli kapat ve hakký kullanýldý olarak iþaretle
        AdPanel.SetActive(false);
        hasRevivedThisLevel = true;

        // 2. Oyuncuya Kalkan Ver! (PlayerController içindeki ActivatePowerUp fonksiyonunu public yapýp çaðýrmalýyýz)
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ActivateShieldFromRevive(); // (Bunu birazdan PlayerController içine ekleyeceðiz)
        }

        // 3. Zamaný tekrar baþlat
        Time.timeScale = 1f;
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
        if (Time.timeScale > 0f)
        {
            yield return new WaitForSeconds(0.8f);
        }

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

    // TUTORIAL PANELS SETTINGS //

    private void StartTutorial()
    {
        isTutorialWaiting = true;
        Time.timeScale = 0f; // Zamaný durdur, engeller gelmesin

        if (TutorialPanel != null)
        {
            TutorialPanel.SetActive(true);
            animHand.Play("hand");
        }
    }

    public void ShowPowerUpTutorial()
    {
        isPowerUpTutorialWaiting = true;
        Time.timeScale = 0f; // PowerUp ekrana girdiði an zamaný durdur

        PowerUpTutorialPanel?.SetActive(true);
    }

    public void ShowSlowMotionTutorial()
    {
        isSlowMotionTutorialWaiting = true;
        Time.timeScale = 0f;

        SlowMotionTutorialPanel?.SetActive(true);

    }

    void ShowObstacleShiftPanel()
    {
        Time.timeScale = 0f;
        isObstacleShiftWaiting = true;
        if (ObstacleShiftPanel != null)
        {
            ObstacleShiftPanel.SetActive(true);
            animShift.Play("Shift");
        }
    }

    void ShowDarkModePanel()
    {
        Time.timeScale = 0f;
        isDarkModeWaiting = true;

        DarkModePanel?.SetActive(true);
    }

    public void ShowGlitchTutorialPanel()
    {
        Time.timeScale = 0f;
        isGlitchWaiting = true;

        GlitchTutorialPanel?.SetActive(true);

    }

    //----------------------------------------------------//

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

    void TurnOnTheLights()
    {
        if (globalLight != null)
        {
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, 0f, Time.deltaTime * fadeSpeed);
        }

        if (pipsyAuraLight != null)
        {
            pipsyAuraLight.intensity = Mathf.Lerp(pipsyAuraLight.intensity, 1f, Time.deltaTime * fadeSpeed);
        }

        // Iþýklar hedefine (yaklaþýk olarak) ulaþtýysa Lerp döngüsünü durdur
        if (globalLight.intensity <= 0.05f && pipsyAuraLight.intensity >= 0.95f)
        {
            globalLight.intensity = 0f; // Tam sayýya sabitle
            pipsyAuraLight.intensity = 1f; // Tam sayýya sabitle
            isLightTransitioning = false; // Döngüyü tamamen kapat
        }
    }

    public void ActivateLightPowerUp(float duration)
    {
        // Eðer Pipsy zaten aydýnlýktayken 2. bir ýþýk daha alýrsa, süreyi baþtan baþlatmak için eskisini durduruyoruz
        if (lightPowerUpCoroutine != null)
        {
            StopCoroutine(lightPowerUpCoroutine);
        }

        isLightTransitioning = false;

        // Iþýklandýrma senaryosunu baþlat
        lightPowerUpCoroutine = StartCoroutine(LightPowerUpRoutine(duration));
    }

    private IEnumerator LightPowerUpRoutine(float duration)
    {
        float transitionSpeed = 4f;

        while (globalLight.intensity < 0.95f)
        {
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, 1f, Time.deltaTime * transitionSpeed);

            if (pipsyAuraLight != null)
                pipsyAuraLight.intensity = Mathf.Lerp(pipsyAuraLight.intensity, 0f, Time.deltaTime * transitionSpeed);

            yield return null;
        }

        globalLight.intensity = 1f;
        if (pipsyAuraLight != null) pipsyAuraLight.intensity = 0f;

        yield return new WaitForSeconds(duration);

        isLightTransitioning = true;
    }

    // --- GEÇÝÞ REKLAMI VE GERÝ SAYIM FONKSÝYONLARI ---

    private IEnumerator AdCountdownRoutine()
    {
        int counter = 3;
        while (counter > 0)
        {
            if (AdTimerText != null)
                AdTimerText.text = counter.ToString();

            yield return new WaitForSecondsRealtime(1f); // Zaman durduðu için gerçek saniyeyi sayar
            counter--;
        }

        // 5 saniye dolduysa paneli kapat ve reklamý göster
        InterstitialTimerPanel.SetActive(false);

        if (AdManager.instance != null)
        {
            AdManager.instance.ShowInterstitialAd(); // Reklamý patlat
        }
        else
        {
            ShowRealLevelCompletePanel(); // AdManager yoksa direkt paneli aç
        }
    }

    // Oyuncu "Reklamý Geç" butonuna basarsa
    public void SkipAdButton()
    {
        if (adTimerCoroutine != null) StopCoroutine(adTimerCoroutine); // Sayacý durdur
        InterstitialTimerPanel.SetActive(false); // Reklam panelini kapat
        ShowRealLevelCompletePanel(); // Reklamý atlattýðý için direkt skora (tebrikler paneline) yönlendir
    }

    // AdManager'daki reklam bitince/kapatýlýnca GameManager'a dönecek olan fonksiyon
    public void ProceedToNextLevelAfterAd()
    {
        ShowRealLevelCompletePanel();
    }

    // Asýl "Bölüm Tamamlandý" panelini güvenli bir þekilde ekrana getiren merkez fonksiyon
    private void ShowRealLevelCompletePanel()
    {
        Time.timeScale = 0f;

        if (LevelCompletePanel != null)
        {
            LevelCompletePanel.SetActive(true);
        }
    }

    public void ClickWatchAdButton()
    {
        // Oyun yöneticisi, ölümsüz AdManager'ý bulup reklamý tetikler
        if (AdManager.instance != null)
        {
            AdManager.instance.ShowRewardedAd();
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}