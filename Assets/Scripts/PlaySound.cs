using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public void Play(Soundy soundy)
    {
        var randomSoundIndex = Random.Range(0, soundy.Sounds.Count);
        var sound = soundy.Sounds[randomSoundIndex];

        AudioController.Instance.PlaySound(sound, false, Random.Range(soundy.MinVolume, soundy.MaxVolume), Random.Range(soundy.MinPitch, soundy.MaxPitch));
    }
}
