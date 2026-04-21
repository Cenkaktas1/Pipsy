using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private bool isGamePaused = false;
    [SerializeField] private GameObject PausePanel;

    [SerializeField] private TextMeshProUGUI ScoreText;
    private int score = 0;

    [SerializeField] private GameObject GameOverPanel;
    public bool isGameOver = false;

    public float CurrentObstacleSpeed = 5f;
    public float MaxObstacleSpeed = 15f;
    public float IncreaseRate = 0.2f;

    [SerializeField] private TextMeshProUGUI HighestScoreText;
    private int HighestScore = 0;

    [Header("Game Over Skorlar")]
    [SerializeField] private TextMeshProUGUI endGameScoreText;
    [SerializeField] private TextMeshProUGUI FinalHighScoreText;
    [SerializeField] private TextMeshProUGUI NewRecord;

    void Awake()
    {
        // Eðer sahnede bir GameManager varsa ve bu o deðilse, kendini yok et.
        // Eðer yoksa, instance bu olsun. (Güvenlik duvarý)
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 120;
        if (PausePanel != null) PausePanel.SetActive(false); // Baþlangýçta duraklatma panelini gizle
        if(GameOverPanel != null) GameOverPanel.SetActive(false); // Baþlangýçta oyun bitti panelini gizle

        Time.timeScale = 1f; // Oyun baþlar baþlamaz zaman akýþýný normal hale getir
        UpdateScore(); // Skoru baþlangýçta güncelle

        CurrentObstacleSpeed = 5f;

        HighestScore = PlayerPrefs.GetInt("HighScore", 0);

        bool isMusicOn = PlayerPrefs.GetInt("MusicState", 1) == 1;
        bool isEffectOn = PlayerPrefs.GetInt("EffectState", 1) == 1;

        if (!isMusicOn)
        {
            AudioManager.instance.MusicSource.mute = true;
        }
        if(!isEffectOn)
        {
            AudioManager.instance.EffectSource.mute = true;
        }

        UpdateHighScoreUI();
    }

    public void AddScore()
    {
        if (isGameOver) return; // Oyun bittiðinde skor ekleme iþlevini devre dýþý býrak
        score++;
        UpdateScore();

        if (score > HighestScore)
        {
            HighestScore = score;
            PlayerPrefs.SetInt("HighScore", HighestScore); // Yeni rekoru "HighScore" anahtarýyla kalýcý olarak kaydet.
            UpdateHighScoreUI();
        }
        if (CurrentObstacleSpeed < MaxObstacleSpeed) CurrentObstacleSpeed += IncreaseRate;
    }

    private void UpdateScore()
    {
        if(ScoreText != null)
        {
            ScoreText.text = score.ToString();
        }
    }

    public void PauseGame()
    {
        if (isGameOver) return; // Oyun bittiðinde duraklatma iþlevini devre dýþý býrak

        isGamePaused = !isGamePaused;
        if(isGamePaused)
        {
            Time.timeScale = 0f; // Oyun durur
            PausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f; // Oyun devam eder
            PausePanel.SetActive(false);
        }
    }

    public void GameOver()
    {   
        if(isGameOver) return; // Oyun zaten bittiyse tekrar çalýþtýrma
        isGameOver = true;
        StartCoroutine(GameOverBool());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Oyun devam eder
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Mevcut sahneyi yeniden yükle
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void UpdateHighScoreUI()
    {
        HighestScoreText.text = "HI: " + HighestScore.ToString();
    }

    private IEnumerator GameOverBool()
    {
        // Animasyonun oynamasý için bekle 
        yield return new WaitForSeconds(0.8f);

        // 2. Bekleme süresi bittikten sonra zamaný durdur ve paneli aç
        Time.timeScale = 0f;
        GameOverPanel.SetActive(true);
        endGameScoreText.text = "Score: " + score.ToString();
        NewRecord.gameObject.SetActive(false);

        int bestScore = PlayerPrefs.GetInt("HighScore", 0);

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("HighScore", bestScore);
            PlayerPrefs.Save();
            FinalHighScoreText.text = "HI: " + bestScore.ToString();
            NewRecord.gameObject.SetActive(true);
        }
        else
        {
            FinalHighScoreText.text = "HI: " + bestScore.ToString();
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save(); // Uygulama kapanýrken PlayerPrefs verilerini kaydet
    }
}
