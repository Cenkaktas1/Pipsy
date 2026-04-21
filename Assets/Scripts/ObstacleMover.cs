using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private float PowerSpeedY = 5f;
    private float PowerSpeedX = 3.5f;
    private float Timer = 0f;

    void Update()
    {
        if (gameObject.tag == "Cyan" || gameObject.tag == "Magenta")
        {
            if (GameManager.instance.isGameOver) return; // Oyun bittiðinde engeller hareket etmeyi durdurur

            transform.Translate(Vector3.down * GameManager.instance.CurrentObstacleSpeed * Time.deltaTime);

            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
        else if (gameObject.tag == "PowerUp")
        {   
            if (GameManager.instance.isGameOver) return; // Oyun bittiðinde power-up hareket etmeyi durdurur

            Timer += Time.deltaTime;
            float newY = transform.position.y - (PowerSpeedY * Time.deltaTime);
            float newX = Mathf.Sin(PowerSpeedX * Timer) * 1.7f;
            gameObject.transform.position = new Vector3(newX, newY, 0);

            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }
}
