using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public SoundEnum Sound;

    public void Play()
    {
        AudioController.Instance.PlaySound(Sound, false, Random.Range(.8f, 1.2f), Random.Range(.5f, 1f));
    }
}
