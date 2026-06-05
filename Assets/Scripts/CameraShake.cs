using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Script'e her yerden kolayca eriþebilmek için Singleton tanýmlýyoruz
    public static CameraShake instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Dýþarýdan çaðrýlacak ana fonksiyon (Varsayýlan deðerler: 0.15 saniye süre, 0.2f þiddet)
    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        // Kameranýn orijinal pozisyonunu kaydet
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Rastgele X ve Y deðerleri üret
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Kamerayý orijinal pozisyonunun etrafýnda rastgele sars
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            // Time.unscaledDeltaTime kullanýyoruz ki oyunda SlowMotion (Aðýr Çekim) aktifse bile kamera normal hýzda titresin
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        // Süre bitince kamerayý tam olarak eski, pürüzsüz orijinal yerine geri oturt
        transform.localPosition = originalPos;
    }
}