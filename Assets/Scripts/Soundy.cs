using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Soundy", menuName = "ScriptableObjects/Soundy", order = 1)]
public class Soundy : ScriptableObject
{
    public List<SoundEnum> Sounds;
    public float MinVolume = 1f;
    public float MaxVolume = 1f;
    public float MinPitch = 1f;
    public float MaxPitch = 1f;
}
