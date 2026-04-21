using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
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
    }

    public void StartGame()
    {
        // Oyun sahnesine geçiþ yap
        SceneManager.LoadScene("GameScene");
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
