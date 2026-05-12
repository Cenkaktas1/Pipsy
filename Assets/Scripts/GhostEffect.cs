using System.Collections;
using UnityEngine;

public class GhostEffect : MonoBehaviour
{
    [Header("Ghost Ayarlarý")]
    public float ghostDelay = 0.05f;
    public float destroyTime = 0.5f;
    [Range(0.1f, 1f)]
    public float ghostAlpha = 0.5f; // Sabit renk yerine, sadece saydamlýk oranýný belirliyoruz

    private bool isGhosting = false;
    private SpriteRenderer playerSprite;

    void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
    }

    public void StartGhosting()
    {
        isGhosting = true;
        StartCoroutine(SpawnGhosts());
    }

    public void StopGhosting()
    {
        isGhosting = false;
    }

    private IEnumerator SpawnGhosts()
    {
        while (isGhosting)
        {
            CreateGhost();
            yield return new WaitForSecondsRealtime(ghostDelay);
        }
    }

    private void CreateGhost()
    {
        GameObject ghostObj = new GameObject("GhostSilhouette");
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;

        SpriteRenderer sr = ghostObj.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite.sprite; // Pipsy'nin o anki þeklini al

        sr.material = playerSprite.material; // Pipsy'nin materyal/shader'ýný birebir kopyala
        sr.sortingLayerName = playerSprite.sortingLayerName;

        // DÝNAMÝK RENK: Pipsy o an Magenta ise Magenta, Cyan ise Cyan olur!
        Color currentColor = playerSprite.color;
        sr.color = new Color(currentColor.r, currentColor.g, currentColor.b, ghostAlpha); // Rengi kopyala, saydamlýðý ayarla

        sr.sortingOrder = playerSprite.sortingOrder - 1;

        StartCoroutine(FadeAndDestroy(sr, ghostObj));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer sr, GameObject obj)
    {
        float elapsedTime = 0f;
        Color startColor = sr.color;

        while (elapsedTime < destroyTime)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float currentAlpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / destroyTime);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);

            yield return null;
        }

        Destroy(obj);
    }
}