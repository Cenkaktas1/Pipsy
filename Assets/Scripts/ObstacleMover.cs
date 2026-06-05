using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private float PowerSpeedY = 5f;
    private float PowerSpeedX = 3.5f;
    private float Timer = 0f;

    private float starSpeed = 0.8f;

    [Header("Çapraz Kayma Ayarlarý (Sadece Cyan/Magenta)")]
    public float shiftTriggerY = 2.0f; // Engelin kaymaya baþlayacaðý Y seviyesi
    public float horizontalSpeed = 3f; // Yatay geçiþ hýzý

    private bool canShiftX = false;
    private float targetX;
    private bool isShifting = false;
    private bool hasShifted = false;

    void Start()
    {
        // Engel doðduðunda nerede olduðuna bakar ve tam tersi yönü hedef olarak belirler.
        // Eðer engel 1.5'te doðduysa hedef -1.5 olur.
        if (transform.position.x > 0)
        {
            targetX = -1.5f;
        }
        else
        {
            targetX = 1.5f;
        }

        if (LevelManager.currentLevel != null && LevelManager.currentLevel.canObstaclesShift)
        {
            if (Random.value > 0.5f) // %50 þans
            {
                canShiftX = true;
            }
        }
    }

    void Update()
    {
        if (gameObject.tag == "Cyan" || gameObject.tag == "Magenta")
        {
            if (GameManager.instance.isGameOver) return; // Oyun bittiðinde engeller hareket etmeyi durdurur

            transform.Translate(Vector3.down * GameManager.instance.CurrentObstacleSpeed * Time.deltaTime);

            if (canShiftX && !hasShifted && !isShifting && transform.position.y <= shiftTriggerY)
            {
                isShifting = true;
            }

            if (isShifting)
            {
                float step = horizontalSpeed * Time.deltaTime;
                float newX = Mathf.MoveTowards(transform.position.x, targetX, step);

                // Y deðerine dokunmadan çünkü Translate ile zaten düþüyor X'i güncelliyoruz.
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);

                // Hedef noktaya (-1.5 veya -1.5 tam olarak ulaþýldýysa yatay hareketi tamamen kilitle.
                if (Mathf.Abs(transform.position.x - targetX) < 0.001f)
                {
                    isShifting = false;
                    hasShifted = true;
                }
            }

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

        else if (gameObject.tag == "Light2D")
        {
            if (GameManager.instance.isGameOver || GameManager.instance.isLevelComplete) return;

            transform.Translate(Vector3.down * GameManager.instance.CurrentObstacleSpeed * starSpeed * Time.deltaTime);

            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }

        else if (gameObject.tag == "GlitchLine")
        {
            if (GameManager.instance.isGameOver || GameManager.instance.isLevelComplete) return;

            // Glitch çizgisi normal engellerle ayný hýzda aþaðý insin
            transform.Translate(Vector3.down * GameManager.instance.CurrentObstacleSpeed * Time.deltaTime);

            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }
}
