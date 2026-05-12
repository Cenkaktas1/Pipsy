using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject LevelPanel;
    [SerializeField] private GameObject Pipsy;

    [SerializeField] private TextMeshProUGUI HighestScoreText;

    [SerializeField] private ParticleSystem particle;

    [Header("Müzik ve Ses Spritelarý")]
    [SerializeField] private Sprite musicOn;
    [SerializeField] private Sprite musicOff;
    [SerializeField] private Sprite soundOn;
    [SerializeField] private Sprite soundOff;

    [Header("UI Referanslarý")]
    [SerializeField] private Image musicBtnImage;
    [SerializeField] private Image soundBtnImage;

    private void Start()
    {
        int Score = PlayerPrefs.GetInt("HighScore", 0);

        if (HighestScoreText != null)
        {
            HighestScoreText.text = "HI: " + Score.ToString();
        }

        UpdateAudioUI();
        MainMenuPanel.SetActive(true);
        LevelPanel.SetActive(false);
        Pipsy.SetActive(true);
    }

    public void OpenLevelPanel()
    {
        // Level paneline geçiþ yap
        MainMenuPanel?.SetActive(false);
        LevelPanel?.SetActive(true);
        if (HighestScoreText != null) HighestScoreText.enabled = false;
        Pipsy?.SetActive(false);
    }
    public void BackToMainMenu()
    {
        LevelPanel?.SetActive(false);
        MainMenuPanel?.SetActive(true);
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
        if (musicBtnImage != null)
        {
            musicBtnImage.sprite = AudioManager.instance.isMusicOn ? musicOn : musicOff;
        }

        if (soundBtnImage != null)
        {
            soundBtnImage.sprite = AudioManager.instance.isEffectOn ? soundOn : soundOff;
        }
    }
}
