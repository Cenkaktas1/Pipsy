using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Obstacles")]
    [SerializeField] private GameObject prefabMagenta;
    [SerializeField] private GameObject prefabCyan;
    public float spawnRate = 1.5f;  // Engel çýkma süresi
    public float timer = 0f;
    private float[] spawnPointsX = { -1.5f, 1.5f };

    [Header("PowerUps and SlowMotion")]
    [SerializeField] private GameObject[] prefabs;
    private float powerUpSpawnRate = 15f;
    private float slowMotionSpawnRate = 15f;

    [Header("Stars")]
    [SerializeField] private GameObject prefabStar;
    private int starSpawnChance = 0;    

    void Start()
    {
        if (LevelManager.currentLevel != null)
        {
            spawnRate = LevelManager.currentLevel.obstacleSpawnRate;
            starSpawnChance = LevelManager.currentLevel.starSpawnChance;

            // Sadece bu levelin izni varsa PowerUp üretmeye baþla
            if (LevelManager.currentLevel.canSpawnPowerUps || LevelManager.currentLevel.canSpawnSlowMotion)
            {
                powerUpSpawnRate = Random.Range(10f, 15f);
                StartCoroutine(SpawnPowerUp());
            }
        }
    }

    void Update()
    {
        // Oyun bittiyse veya level geçildiyse üretim yapmayý durdur
        if (GameManager.instance.isGameOver || GameManager.instance.isLevelComplete) return;

        timer += Time.deltaTime;

        float CurrentSpawnRate = spawnRate * (5f / GameManager.instance.CurrentObstacleSpeed);

        if(timer >= CurrentSpawnRate)
        {
            SpawnEntity();
            timer = 0f;
        }
    }

    private void SpawnEntity()
    {        
        int randomIndex = Random.Range(0, spawnPointsX.Length);
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

    private IEnumerator SpawnPowerUp()
    {
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
}
