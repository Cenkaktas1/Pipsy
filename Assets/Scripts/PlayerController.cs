using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float leftLaneX = -1.5f;
    public float rightLaneX = 1.5f;

    public Color colorLeft = Color.magenta;
    public Color colorRight = Color.cyan;

    [SerializeField] private Sprite Cyan;
    [SerializeField] private Sprite Magenta;

    // Geçiþin ne kadar hýzlý olacaðýný belirler
    public float moveSpeed = 15f;

    private bool isLeft = true;
    private bool isGameOver = false;

    // --- YENÝ: GLITCH (AYNA) MODU DEÐÝÞKENÝ ---
    public bool isMirrorModeActive = false;

    private float targetX; // Gitmek istediðimiz hedef X noktasý
    private Color targetColor; // Dönüþmek istediðimiz hedef renk
    private SpriteRenderer spriteRenderer;

    private ParticleSystem particle; // Karakterin arkasýnda býraktýðý izi kontrol etmek için
    private Color currentTailColor;

    private Animator anim;

    private bool isPowerUp = false;
    private float PowerUpTimer = 5f;

    [Header("Glitch Efektleri")]
    [SerializeField] private ParticleSystem glitchParticle; // Yeni patlama partikülü

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particle = GetComponentInChildren<ParticleSystem>();
        anim = GetComponent<Animator>();
        SetTargetStates();
        StartCoroutine(BlinkTimer());

        isGameOver = false;

        // Oyun baþlar baþlamaz karakteri direkt yerine koyuyoruz
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        spriteRenderer.color = Color.white;

        currentTailColor = isLeft ? colorLeft : colorRight;// kuyruðun baþlangýç rengini belirle
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // Oyun durdurulmuþsa hiçbir þey yapma

        if (Input.GetMouseButtonDown(0) && !GameManager.instance.isGameOver)
        {
            // farenin veya parmaðýn UI üzerinde olup olmadýðýný kontrol eder.
            if (IsPointerOverUI()) return;

            isLeft = !isLeft;
            SetTargetStates();

            if (AudioManager.instance.SwitchSound != null)
                AudioManager.instance.PlayEffect(AudioManager.instance.SwitchSound);
        }

        // Smooth Movement
        // Her karede karakterin X pozisyonunu hedefe doðru biraz daha yaklaþtýrýyoruz.
        float currentX = Mathf.Lerp(transform.position.x, targetX, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(currentX, transform.position.y, transform.position.z);

        //  Yumuþak Renk Geçiþi
        if (isPowerUp)
            currentTailColor = Color.green;
        else
            currentTailColor = Color.Lerp(currentTailColor, targetColor, moveSpeed * Time.deltaTime);

        if (particle != null)
        {
            var main = particle.main;
            main.startColor = currentTailColor;
        }
    }

    private bool IsPointerOverUI()
    {
        // Eðer ekranda bir parmak dokunuþu varsa
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        // Eðer parmak yoksa fare týklamasýný kontrol et
        return EventSystem.current.IsPointerOverGameObject();
    }

    void SetTargetStates()
    {
        // Fiziksel olarak gitmek istediðimiz X koordinatý (Bu hep ayný kalmalý)
        targetX = isLeft ? leftLaneX : rightLaneX;

        // --- KRÝTÝK AYAR: ANIMASYON RENGÝNÝ HESAPLA ---
        // Eðer ayna modu aktifse, animatöre fiziksel konumun TERSÝ deðerini gönderiyoruz.
        // Böylece soldayken saðýn animasyonu (Cyan), saðdayken solun animasyonu (Magenta) oynuyor.
        bool visualIsLeft = isMirrorModeActive ? !isLeft : isLeft;

        // Kuyruk/Partikül rengi için hedefi belirle
        if (isMirrorModeActive)
        {
            targetColor = isLeft ? colorRight : colorLeft;
        }
        else
        {
            targetColor = isLeft ? colorLeft : colorRight;
        }

        // Animatördeki "isLeft" parametresine artýk fiziksel deðiþkeni deðil, 
        // ayna modunu hesaba katan "visualIsLeft" deðerini gönderiyoruz.
        if (anim != null)
        {
            anim.SetBool("isLeft", visualIsLeft);
        }
    }

    public void ToggleMirrorMode()
    {
        isMirrorModeActive = !isMirrorModeActive;
        SetTargetStates(); // Mod deðiþtiði an, ekrana dokunulmasýný beklemeden rengi anýnda güncelle
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("PowerUp"))
        {
            StartCoroutine(ActivatePowerUp()); // 5 saniye boyunca yanmama özelliði etkinleþtir
            AudioManager.instance.PlayEffect(AudioManager.instance.PowerUpSound);
            Destroy(collision.gameObject);
        }

        else if (collision.CompareTag("Star"))
        {
            GameManager.instance.AddStar();
            AudioManager.instance.PlayEffect(AudioManager.instance.CollectSound);
            Destroy(collision.gameObject);
        }

        else if (collision.CompareTag("SlowMotion"))
        {
            GameManager.instance.ActivateSlowMotion();
            AudioManager.instance.PlayEffect(AudioManager.instance.SlowMotionSound);
            Destroy(collision.gameObject);
        }

        //CYAN ÇARPIÞMASI
        else if (collision.CompareTag("Cyan"))
        {
            // Güvende olma durumu: Normalde saðda (!isLeft) olmak güvendir. Ayna modunda ise solda (isLeft) olmak güvendir.
            bool isSafe = isMirrorModeActive ? isLeft : !isLeft;

            if (isSafe || isPowerUp)
            {
                GameManager.instance.AddScore();
                AudioManager.instance.PlayEffect(AudioManager.instance.ScoreSound);
            }
            else
            {
                anim.SetTrigger("doShock");
                GameManager.instance.GameOver();
                AudioManager.instance.PlayEffect(AudioManager.instance.GameOverSound);
            }
        }

        // MAGENTA ÇARPIÞMASI
        else if (collision.CompareTag("Magenta"))
        {
            // Güvende olma durumu: Normalde solda (isLeft) olmak güvendir. Ayna modunda ise saðda (!isLeft) olmak güvendir.
            bool isSafe = isMirrorModeActive ? !isLeft : isLeft;

            if (isSafe || isPowerUp)
            {
                GameManager.instance.AddScore();
                AudioManager.instance.PlayEffect(AudioManager.instance.ScoreSound);
            }
            else
            {
                anim.SetTrigger("doShock");
                GameManager.instance.GameOver();
                AudioManager.instance.PlayEffect(AudioManager.instance.GameOverSound);
            }
        }

        else if (collision.CompareTag("Light2D"))
        {
            AudioManager.instance.PlayEffect(AudioManager.instance.PowerUpSound);
            GameManager.instance.ActivateLightPowerUp(3.5f);
            Destroy(collision.gameObject);
        }

        else if (collision.CompareTag("GlitchLine"))
        {
            AudioManager.instance.PlayEffect(AudioManager.instance.GlitchSound);

            ToggleMirrorMode();

            if (CameraShake.instance != null)
            {
                CameraShake.instance.Shake(0.15f, 0.2f);
            }

            if(glitchParticle != null)
            {
                glitchParticle.transform.position = transform.position; // Efekti karakterin üstüne taþý
                glitchParticle.Play(); // Patlat!
            }
        }
    }

    private IEnumerator BlinkTimer()
    {
        while (!isGameOver)
        {
            float waitTime = Random.Range(2.5f, 4f);
            yield return new WaitForSeconds(waitTime);

            anim.SetTrigger("doBlink"); // Animator'a Göz Kýrp komutu gönder
        }
    }

    private IEnumerator ActivatePowerUp()
    {
        float WarningTime = 0f;
        float WarningRate = 0.15f;
        isPowerUp = true;

        if (anim != null)
        {
            anim.speed = 2f;
            anim.SetBool("isPowerUp", true);
        }
        yield return new WaitForSeconds(PowerUpTimer - 1.5f); // Power-up süresinin son 1.5 saniyesi için uyarý vermesi için sadece 3.5 saniye bekleniyor.

        while (WarningTime < 1.5f)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Sprite'ý görünmez yaparak yanýp sönme efekti oluþtur
            yield return new WaitForSeconds(WarningRate);
            WarningTime += WarningRate;
        }

        spriteRenderer.enabled = true; // Power-up süresi bittiðinde sprite'ý tekrar görünür yap

        isPowerUp = false;
        if (anim != null)
        {
            anim.SetBool("isPowerUp", false);
            anim.speed = 1f;
        }
    }

    public void ActivateShieldFromRevive()
    {
        // 1. Eðer animatör tanýmlýysa onu baþlangýç (hayatta kalma) durumuna sýfýrla
        if (anim != null)
        {
            anim.ResetTrigger("doShock"); // Þok tetikleyicisini iptal et
            anim.Rebind(); // Animatörü oyunun ilk baþladýðý o varsayýlan hale döndür
            anim.Update(0f); // Deðiþikliði anýnda ekrana yansýt
        }

        // 2. Eski yazdýðýn ActivatePowerUp coroutine'ini tetikler (5 saniye ölümsüzlük)
        StartCoroutine(ActivatePowerUp());
    }
}