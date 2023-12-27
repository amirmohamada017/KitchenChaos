using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { private set; get; }
    
    private const string PlayerPrefsMusicVolume = "MusicVolume";
    private AudioSource _audioSource;
    private float _volume = .3f;

    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        _volume = PlayerPrefs.GetFloat(PlayerPrefsMusicVolume, 1f);
        _audioSource.volume = _volume;
    }

    public void ChangeVolume()
    {
        _volume += .1f;
        
        if (_volume > 1.05)
            _volume = 0;

        _audioSource.volume = _volume;
        
        PlayerPrefs.SetFloat(PlayerPrefsMusicVolume, _volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return _volume;
    }
}
