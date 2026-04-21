using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource EffectSource;

    [Header("Effects")]
    public AudioClip SwitchSound;
    public AudioClip ScoreSound;
    public AudioClip GameOverSound;
    public AudioClip PowerUpSound;
    public AudioClip SelectSound;

    public bool isMusicOn;
    public bool isEffectOn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ve sahneler deðiþse bile yok etme
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicState", 1) == 1;
        isEffectOn = PlayerPrefs.GetInt("EffectState", 1) == 1;

        MusicSource.mute = !isMusicOn;
        EffectSource.mute = !isEffectOn;
    }

    public void MusicOn()
    {
        isMusicOn = !isMusicOn;

        PlayerPrefs.SetInt("MusicState", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        MusicSource.mute = !isMusicOn; // Müzikleri açýp kapama butonu
    }

    public void EffectsOn()
    {
        isEffectOn = !isEffectOn;

        PlayerPrefs.SetInt("EffectState", isEffectOn ? 1 : 0);
        PlayerPrefs.Save();
        EffectSource.mute = !isEffectOn; // Efektleri açýp kapama butonu
    }

    public void PlayEffect(AudioClip effect)
    {
        if(!EffectSource.mute && effect != null)
            EffectSource.PlayOneShot(effect); // Efektleri tek seferlik çalmak için gerekli metod
    }  
}
