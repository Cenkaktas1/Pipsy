using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelBtnManager : MonoBehaviour
{
    public LevelData levelData;
    public TextMeshProUGUI leveltxt;
    public Button levelbtn;
    public GameObject lockUI;

    void Start()
    {
        if (levelData != null)
        {
            leveltxt.text = levelData.isEndless ? "Endless" : levelData.levelIndex.ToString();

            // Bölüm kilitli mi kontrol et PlayerPrefs kullanarak
            CheckLockStatus();
        }
    }

    void CheckLockStatus()
    {
        if (levelData == null || levelData.isEndless)
        {
            if (lockUI != null) lockUI.SetActive(false);
            return;
        }

        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        if (levelData.levelIndex > reachedLevel)
        {
            if (lockUI != null) lockUI.SetActive(true); // level daha açýlmamýþsa kilitli göster
            levelbtn.interactable = false; // Butonu týklanamaz yap
        }
        else
            if (lockUI != null) lockUI.SetActive(false); //level açýlmýþsa kilidi kaldýr
    }

    public void OnLevelButtonClick()
    {
        // Seçilen veriyi statik sýnýfa aktar
        LevelManager.currentLevel = levelData;

        // Oyun sahnesine geçiþ yap
        SceneManager.LoadScene("GameScene");
    }
}
