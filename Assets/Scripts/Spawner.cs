using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Obstacles")]
    [SerializeField] private GameObject prefabMagenta;
    [SerializeField] private GameObject prefabCyan;
    public float spawnRate = 1.5f;  // Engel çýkma süresi
    public float timer = 0f;
    private float[] spawnPointsX = { -1.5f, 1.5f };

    [Header("PowerUps")]
    [SerializeField] private GameObject prefabPowerUp;
    private float powerUpSpawnRate = 15f;

    void Start()
    {   
        powerUpSpawnRate = Random.Range(10f, 15f);
        StartCoroutine(SpawnPowerUp());
}

    void Update()
    {
        timer += Time.deltaTime;

        float CurrentSpawnRate = spawnRate * (5f / GameManager.instance.CurrentObstacleSpeed);

        if(timer >= CurrentSpawnRate)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    private void SpawnObstacle()
    {        
        int randomIndex = Random.Range(0, spawnPointsX.Length);
        float randomX = spawnPointsX[randomIndex];
        GameObject prefab = Random.value < 0.5f ? prefabCyan : prefabMagenta;

        Vector3 SpawnPoint = new Vector3(randomX, 6f, 0);
        Instantiate(prefab, SpawnPoint, Quaternion.identity);
    }

    private IEnumerator SpawnPowerUp()
    {
        yield return new WaitForSeconds(powerUpSpawnRate);

        while (!GameManager.instance.isGameOver)
        {
            powerUpSpawnRate = Random.Range(10f, 15f); // Power-up çýkma süresi
            Vector3 SpawnPoint = new Vector3(0, 6f, 0);
            Instantiate(prefabPowerUp, SpawnPoint, Quaternion.identity);

            yield return new WaitForSeconds(powerUpSpawnRate);
        }
    }
}
