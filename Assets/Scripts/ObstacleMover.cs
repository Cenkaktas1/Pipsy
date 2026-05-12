using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private float PowerSpeedY = 5f;
    private float PowerSpeedX = 3.5f;
    private float Timer = 0f;

    private float starSpeed = 0.8f;

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

        else if (gameObject.tag == "PowerUp" || gameObject.tag == "SlowMotion")
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

        else if (gameObject.tag == "Star")
        {
            if (GameManager.instance.isGameOver || GameManager.instance.isLevelComplete) return;

            // Yýldýzýn hýzýný, oyunun o anki engel hýzýna göre hesapla (Daha yavaþ düþmesi için)
            transform.Translate(Vector3.down * GameManager.instance.CurrentObstacleSpeed * starSpeed * Time.deltaTime);

            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }
}
