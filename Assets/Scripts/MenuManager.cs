using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject LevelAndEndlessPanel;
    [SerializeField] private GameObject LevelPanel;
    [SerializeField] private GameObject LevelPanel_2;
    [SerializeField] private GameObject Pipsy;

    [SerializeField] private TextMeshProUGUI HighestScoreText;

    [SerializeField] private ParticleSystem particle;

    [Header("Müzik ve Ses Spritelarý")]
    [SerializeField] private Sprite musicOn;
    [SerializeField] private Sprite musicOff;
    [SerializeField] private Sprite soundOn;
    [SerializeField] private Sprite soundOff;

    [Header("UI Referanslarý")]
    [SerializeField] private Image[] musicBtnImage;
    [SerializeField] private Image[] soundBtnImage;

    private void Start()
    {
        int Score = PlayerPrefs.GetInt("HighScore", 0);

        if (HighestScoreText != null)
        {
            HighestScoreText.text = "HI: " + Score.ToString();
        }

        UpdateAudioUI();
        MainMenuPanel?.SetActive(true);
        LevelAndEndlessPanel?.SetActive(false);
        LevelPanel?.SetActive(false);
        LevelPanel_2?.SetActive(false);
        Pipsy?.SetActive(true);
    }

    public  void OpenLevelAndEndlessPanel()
    {
        MainMenuPanel?.SetActive(false);
        LevelAndEndlessPanel?.SetActive(true);
    }

    public void OpenLevelPanel()
    {
        // Level paneline geçiþ yap
        LevelAndEndlessPanel?.SetActive(false);
        LevelPanel_2?.SetActive(false);
        LevelPanel?.SetActive(true);
        if (HighestScoreText != null) HighestScoreText.enabled = false;
        Pipsy?.SetActive(false);
    }

    public void OpenLevelPanel_2()
    {
        LevelPanel?.SetActive(false);
        LevelPanel_2?.SetActive(true);
    }

    public void BackToMainMenu()
    {
        LevelPanel?.SetActive(false);
        LevelPanel_2?.SetActive(false);
        LevelAndEndlessPanel?.SetActive(false);
        MainMenuPanel?.SetActive(true);
        if (HighestScoreText != null) HighestScoreText.enabled = true;
        Pipsy?.SetActive(true);
    }

    public void BackToLevelAndEndlessPanel()
    {
        LevelPanel?.SetActive(false);
        LevelAndEndlessPanel?.SetActive(true);
        if (HighestScoreText != null) HighestScoreText.enabled = true;
        Pipsy?.SetActive(true);
    }
    public void ChangeMusic()
    {
        AudioManager.instance.MusicOn();
        UpdateAudioUI();
    }

    public void ChangeEffects()
    {
        AudioManager.instance.EffectsOn();
        UpdateAudioUI();
    }

    private void UpdateAudioUI()
    {
        if (musicBtnImage != null && soundBtnImage != null)
        {
            for(int i = 0; i < soundBtnImage.Length; i++)
            {
                musicBtnImage[i].sprite = AudioManager.instance.isMusicOn ? musicOn : musicOff;
                soundBtnImage[i].sprite = AudioManager.instance.isEffectOn ? soundOn : soundOff;
            }
        }
    }
}
