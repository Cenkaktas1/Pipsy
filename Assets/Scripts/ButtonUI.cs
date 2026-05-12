using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    [Header("Level Ayarlarý")]
    public int levelIndex; // Bu buton hangi leveli temsil ediyor?

    [Header("Yýldýz Görselleri")]
    public Image[] stars; // Butonun içindeki 3 yýldýz objesi
    public Sprite filledStarSprite;

    void Start()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        // Eðer bu butonun leveli, oyuncunun ulaþtýðý levelden büyükse (KÝLÝTLÝYSE)
        if (levelIndex > reachedLevel)
        {
            HideStars(); // Yýldýzlarý tamamen ekrandan kaldýr
        }
        else
        {
            LoadAndDisplayStars(); // Kilit açýksa yýldýz sayýsýný kontrol et ve göster
        }
    }

    public void LoadAndDisplayStars()
    {
        // Cihazýn hafýzasýndan bu levelin yýldýzýný çek
        string keyName = "Level_" + levelIndex + "_Stars";
        int savedStars = PlayerPrefs.GetInt(keyName, 0);

        // Döngü ile yýldýzlarý aç veya kapat
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < savedStars)
            {
                stars[i].sprite = filledStarSprite;
            }
        }
    }

    private void HideStars()
    {
        // Döngü ile yýldýz objelerinin Game Object'lerini tamamen kapatýyoruz
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].gameObject.SetActive(false);
            }
        }
    }
}