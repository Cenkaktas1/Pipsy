using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner instance;

    [Header("Obstacles")]
    [SerializeField] private GameObject prefabMagenta;
    [SerializeField] private GameObject prefabCyan;
    public float spawnRate = 1.5f;  // Engel çýkma süresi
    public float timer = 0f;
    private float[] spawnPointsX = { -1.5f, 1.5f };
    private int randomIndex;

    [Header("PowerUps and SlowMotion")]
    [SerializeField] private GameObject[] prefabs;
    private float powerUpSpawnRate = 15f;
    private float slowMotionSpawnRate = 15f;

    [Header("Stars")]
    [SerializeField] private GameObject prefabStar;
    private int starSpawnChance = 0;

    [Header("Light")]
    [SerializeField] private GameObject prefabLight;
    [SerializeField] private float lightSpawnRate = 7f;

    [Header("Glitch")]
    [SerializeField] private GameObject prefabGlitchLine;
    [SerializeField] private float glitchSpawnRate = 12f;
    private bool isObstaclePaused = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        if (LevelManager.currentLevel != null)
        {
            spawnRate = LevelManager.currentLevel.obstacleSpawnRate;
            starSpawnChance = LevelManager.currentLevel.starSpawnChance;
        }
    }

    void Update()
    {
        // Oyun bittiyse veya level geçildiyse üretim yapmayý durdur
        if (GameManager.instance.isGameOver || GameManager.instance.isLevelComplete) return;

        if (isObstaclePaused) return;

        timer += Time.deltaTime;

        float CurrentSpawnRate = spawnRate * (5f / GameManager.instance.CurrentObstacleSpeed);

        if(timer >= CurrentSpawnRate)
        {
            SpawnEntity();
            timer = 0f;
        }
    }

    public void CallPowerUp()
    {
        powerUpSpawnRate = Random.Range(10f, 15f);
        StartCoroutine(SpawnPowerUp());
    }

    public void CallLight()
    {
        lightSpawnRate = Random.Range(9f, 12f);
        StartCoroutine(SpawnLight());
    }

    public void CallGlitch()
    {
        glitchSpawnRate = Random.Range(20f, 25f); // Glitchin ilk gelme süresi biraz uzun olsun ki oyuncu önce bir normal oynasýn
        StartCoroutine(SpawnGlitchLine());
    }

    private void SpawnEntity()
    {        
        randomIndex = Random.Range(0, spawnPointsX.Length);
        float randomX = spawnPointsX[randomIndex];
        GameObject prefab = Random.value < 0.5f ? prefabCyan : prefabMagenta;

        Vector3 SpawnPoint = new Vector3(randomX, 6f, 0);
        Instantiate(prefab, SpawnPoint, Quaternion.identity);

        if (prefabStar != null && GameManager.instance.collectedStars < 3 && Random.Range(0, 100) < starSpawnChance)
        {
            // Yýldýzýn engelle üst üste binmemesi için onu diðer þeritten gönderiyoruz.
            // Eðer engel 0. indekste (solda) ise yýldýz 1. indekste saðda çýkar.
            int starIndex = (randomIndex == 0) ? 1 : 0;
            float starX = spawnPointsX[starIndex];

            // Yýldýzýn pozisyonu
            Vector3 starSpawnPoint = new Vector3(starX, 6f, 0);
            Instantiate(prefabStar, starSpawnPoint, Quaternion.identity);
        }   
    }

    private IEnumerator SpawnLight()
    {
        yield return new WaitForSeconds(lightSpawnRate);

        while (!GameManager.instance.isGameOver && !GameManager.instance.isLevelComplete)
        {
            if (prefabLight != null && LevelManager.currentLevel.isLightAvailable)
            {
                int lightIndex = (randomIndex == 0) ? 1 : 0;
                float lightX = spawnPointsX[lightIndex];

                Vector3 lightSpawnPoint = new Vector3(lightX, 6f, 0);
                Instantiate(prefabLight, lightSpawnPoint, Quaternion.identity);
            }
            yield return new WaitForSeconds(lightSpawnRate);
        }
    }

    private IEnumerator SpawnPowerUp()
    {
        Time.timeScale = 1f;
        yield return new WaitForSeconds(powerUpSpawnRate);
        bool isFirstPowerUp = true;
        bool isFirstSlowMotion = true;

        while (!GameManager.instance.isGameOver && !GameManager.instance.isLevelComplete)
        {
            if (LevelManager.currentLevel.canSpawnPowerUps && LevelManager.currentLevel.canSpawnSlowMotion)
            {
                int randomIndex = Random.Range(0, prefabs.Length);
                powerUpSpawnRate = Random.Range(10f, 15f);
                Vector3 SpawnPoint = new Vector3(0, 6f, 0);
                Instantiate(prefabs[randomIndex], SpawnPoint, Quaternion.identity);
            }

            else if (LevelManager.currentLevel.canSpawnPowerUps)
            {
                powerUpSpawnRate = Random.Range(10f, 15f); // Power-up çýkma süresi
                Vector3 SpawnPoint = new Vector3(0, 6f, 0);
                Instantiate(prefabs[0], SpawnPoint, Quaternion.identity);
            }
            else if (LevelManager.currentLevel.canSpawnSlowMotion)
            {
                slowMotionSpawnRate = Random.Range(1f, 5f); // SlowMotion çýkma süresi
                Vector3 SpawnPoint = new Vector3(0, 6f, 0);
                Instantiate(prefabs[1], SpawnPoint, Quaternion.identity);
            }

            if (isFirstPowerUp && LevelManager.currentLevel != null && LevelManager.currentLevel.isPowerUpTutorial)
            {
                GameManager.instance.ShowPowerUpTutorial();
                isFirstPowerUp = false; // Bir daha sorma
            }

            if(isFirstSlowMotion && LevelManager.currentLevel != null && LevelManager.currentLevel.isSlowMotionTutorial)
            {
                GameManager.instance.ShowSlowMotionTutorial();
                isFirstSlowMotion = false;
            }

            yield return new WaitForSeconds(powerUpSpawnRate);
        }
    }

    private IEnumerator SpawnGlitchLine()
    {
        bool isFirstGlitch = true;
        Time.timeScale = 1f;

        // Ýlk glitch çizgisi gelmeden önce bekle
        yield return new WaitForSeconds(glitchSpawnRate);

        while (!GameManager.instance.isGameOver && !GameManager.instance.isLevelComplete)
        {
            if (prefabGlitchLine != null && LevelManager.currentLevel.isGlitchAvailable)
            {
                // Glitch çizgisi dikey ekraný kapladýðý için X ekseninde 0 (tam orta) noktasýnda doðmalý
                Vector3 glitchSpawnPoint = new Vector3(0f, 6f, 0f);
                Instantiate(prefabGlitchLine, glitchSpawnPoint, Quaternion.identity);

                StartCoroutine(PauseObstaclesRoutine(2.5f));
            }

            if(isFirstGlitch && LevelManager.currentLevel.isGlitchTutorial)
            {
                GameManager.instance.ShowGlitchTutorialPanel();
                isFirstGlitch = false;
            }

            // Bir sonraki glitch çizgisinin ne zaman geleceðini belirle
            // Örneðin her 10-15 saniyede bir normal/ters evren arasýnda geçiþ yapsýn
            glitchSpawnRate = Random.Range(20f, 25f);
            yield return new WaitForSeconds(glitchSpawnRate);
        }
    }

    private IEnumerator PauseObstaclesRoutine(float pauseDuration)
    {
        isObstaclePaused = true; // Engel üretimini kilitle
        timer = 0f; // Zamanlayýcýyý sýfýrla ki, mola biter bitmez aniden engel fýrlamasýn

        yield return new WaitForSeconds(pauseDuration); // Belirlenen süre (örn: 2.5sn) kadar bekle

        isObstaclePaused = false; // Kilidi aç, oyun normal akýþýna dönsün
    }
}
