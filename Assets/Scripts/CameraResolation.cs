using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    // Kameranýn ekrana sýðdýrmasýný istediðin birim geniþlik. 
    // Pipsy'nin sol ve sað þeritlerinin (leftLaneX, rightLaneX) sýðacaðý kadar bir deðer gir.
    // Varsayýlan kamera boyutu genelde 5'tir, bu yüzden geniþlik için 3 veya 4 gibi bir deðer deneyebilirsin.
    public float targetWidth = 3f;

    void Start()
    {
        // Kameranýn boyutunu, ekranýn oranýna (aspect ratio) göre yeniden hesaplar.
        // Bu sayede ekran ne kadar ince olursa olsun yanlardan asla kýrpýlmaz, gerekirse üstten/alttan boþluk býrakýr.
        Camera.main.orthographicSize = targetWidth / Camera.main.aspect;
    }
}
