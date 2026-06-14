using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Pipsy/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Bölüm Bilgileri")]
    public int levelIndex; // Bölüm numarasý
    public bool isTutorial; // Bu bölüm öðretici mi?
    public bool isEndless; // Bu veri sonsuz moda mý ait?

    [Header("Engel Dinamikleri")]
    public float baseObstacleSpeed = 3f; // Bölüm baþladýðýnda engellerin ilk düþüþ hýzý
    public float maxObstacleSpeed = 8f;  // Bu bölümde engellerin ulaþabileceði maksimum hýz
    public float speedIncreaseMultiplier = 0.2f; // Her skor alýndýðýnda hýz ne kadar artacak?

    [Header("Zorluk Ayarlarý")]
    public float obstacleSpawnRate = 1.5f; // Engellerin çýkma sýklýðý
    public int targetScore;

    [Header("PowerUp Ayarlarý")]
    public bool canSpawnPowerUps = false; // Bu levelde powerup çýkacak mý?
    public bool isPowerUpTutorial = false; // Ýlk çýktýðý levelde bilgi verilsin mi?
    public bool canSpawnSlowMotion = false;
    public bool isSlowMotionTutorial = false;

    [Header("Star Settings")]
    [Range(0, 100)] public int starSpawnChance = 20;

    [Header("Özel Mekanikler")]
    public bool canObstaclesShift = false; // Bu bölümde engeller çapraz kayabilir mi?
    public bool isFirstTimeShift = false;

    [Header("Karanlýk Mod")]
    public bool isLevelDark = false;
    public bool isDarkTutorial = false;
    public bool isLightAvailable = false;
    public bool isLightTutorial;

    [Header("Glitch")]
    public bool isGlitchAvailable = false;
    public bool isGlitchTutorial = false;
}
