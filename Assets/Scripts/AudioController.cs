using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    private MusicEnum _currentMusic;

    public List<SoundEnumMap> SoundEffects;
    private Dictionary<SoundEnum, SoundEnumMap> _soundEffectDict;

    public List<MusicEnumMap> Music;
    private Dictionary<MusicEnum, MusicEnumMap> _musicDict;

    private List<AudioSource> _audioSources;
    private List<AudioSource> _musicSources;
    private List<float> _targetVolume;
    private List<float> _previousVolume;
    private float _transitionTime;
    private bool _transitioning;
    private int _activeMusicSourceIndex = -1;

    private void Awake()
    {
        Instance = this;

        _soundEffectDict = new Dictionary<SoundEnum, SoundEnumMap>();
        SoundEffects.ForEach(x => _soundEffectDict.Add(x.name, x));

        _musicDict = new Dictionary<MusicEnum, MusicEnumMap>();
        Music.ForEach(x => _musicDict.Add(x.name, x));

        _audioSources = new List<AudioSource>();
        for (var i = 0; i < 5; i++)
        {
            var x = new GameObject("audio" + i);
            x.transform.SetParent(transform);
            var source = x.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            _audioSources.Add(source);
        }

        _musicSources = new List<AudioSource>();
        _targetVolume = new List<float>();
        _previousVolume = new List<float>();
        for (var i = 0; i < 3; i++)
        {
            var x = new GameObject("music" + i);
            x.transform.SetParent(transform);

            var source = x.AddComponent<AudioSource>();
            source.loop = true;
            source.volume = 0;
            _musicSources.Add(source);

            _targetVolume.Add(0);
            _previousVolume.Add(0);
        }
    }

    public void PlayMusic(MusicEnum music, float volume = 0, bool stopPrevious = false)
    {
        if (music == MusicEnum.None) return;
        if (_currentMusic == music) return;
        _currentMusic = music;
        var m = _musicDict[music];
        //if (volume == 0) volume = m.defaultVolume;

        for (var i = 0; i < _musicSources.Count; i++)
        {
            _previousVolume[i] = _musicSources[i].volume;
        }
        if (stopPrevious)
        {
            _targetVolume[_activeMusicSourceIndex] = 0;
        }
        _activeMusicSourceIndex = _activeMusicSourceIndex < 2 ? _activeMusicSourceIndex + 1 : 0;
        _targetVolume[_activeMusicSourceIndex] = volume;
        _musicSources[_activeMusicSourceIndex].clip = m.sound;
        _musicSources[_activeMusicSourceIndex].Play();
        _transitionTime = 0;
        _transitioning = true;
    }

    public void SetVolume(int musicSourceIndex, float targetVolume)
    {
        _targetVolume[musicSourceIndex] = targetVolume;
        _transitioning = true;
    }

    private void Update()
    {
        if (_transitioning)
        {
            _transitionTime += Time.deltaTime;
            for (var i = 0; i < _musicSources.Count; i++)
            {
                _musicSources[i].volume = Mathf.Lerp(_previousVolume[i], _targetVolume[i], _transitionTime / .5f);
            }
            if (_transitionTime >= .5f)
            {
                _transitioning = false;

                //var otherIndex = _activeMusicSourceIndex == 0 ? 1 : 0;
                //_musicSources[otherIndex].Stop();
            }
        }
    }

    public void StopMusic()
    {
        _currentMusic = MusicEnum.None;
        _transitioning = true;
        _transitionTime = 0;
        _previousVolume[_activeMusicSourceIndex] = _musicSources[_activeMusicSourceIndex].volume;
        _targetVolume[_activeMusicSourceIndex] = 0;
    }

    public void PlaySound(SoundEnum sound, bool loop = false, float volume = 0, float pitch = 1)
    {
        if (sound == SoundEnum.None) return;

        var s = _soundEffectDict[sound];
        if (volume == 0)
        {
            volume = s.defaultVolume;
        }
        var source = GetFreeAudioSource();
        if (source == null) return;

        source.pitch = pitch;

        source.Stop();
        source.clip = s.sound;
        source.volume = volume;
        source.Play();
    }

    private AudioSource GetFreeAudioSource()
    {
        foreach (var source in _audioSources)
        {
            if (!source.isPlaying) return source;
        }

        return null;
    }

    public void PlaySound(int sound)
    {
        PlaySound((SoundEnum)sound);
    }
}

public enum SoundEnum
{
    None = 0,
    Walk = 1,
    WingFlap = 2,
    Meow = 3,
    CatAttack = 4,
    Eat = 5,
}

public enum MusicEnum
{
    None,
    LowIntensity,
    MediumIntensity,
    HighIntensity,
}

[Serializable]
public struct SoundEnumMap
{
    public SoundEnum name;
    public AudioClip sound;
    public float defaultVolume;
}

[Serializable]
public struct MusicEnumMap
{
    public MusicEnum name;
    public AudioClip sound;
    public float defaultVolume;
}
