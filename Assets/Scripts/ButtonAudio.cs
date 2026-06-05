using UnityEngine;

public class MenuAudioController : MonoBehaviour
{

    public void PlaySelectSound()
    {
        if (AudioManager.instance != null && AudioManager.instance.SelectSound != null)
        {
            AudioManager.instance.PlayEffect(AudioManager.instance.SelectSound);
        }
    }
}