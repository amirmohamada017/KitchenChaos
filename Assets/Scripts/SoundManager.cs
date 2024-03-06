using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    private const string PlayerPrefsSoundEffectsVolume = "SoundEffectVolume";

    public static SoundManager Instance { get; private set; }
    
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float _volume = 1f;
    
    private void Start()
    {
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        //Player.Instance.OnPickedSomething += Player_OnPickedSomething;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void Awake()
    {
        Instance = this;
        _volume = PlayerPrefs.GetFloat(PlayerPrefsSoundEffectsVolume, 1f);
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
    {
        var trashCounter = sender as TrashCounter;
        PlaySound(audioClipRefsSO.trash, trashCounter!.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
    {
        var baseCounter = sender as BaseCounter;
        PlaySound(audioClipRefsSO.objectDrop, baseCounter!.transform.position);
    }

    private void Player_OnPickedSomething(object sender, EventArgs e)
    {
        //PlaySound(audioClipRefsSO.objectPickUp, Player.Instance.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        var cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRefsSO.chop, cuttingCounter!.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        var deliveryCounter = DeliveryCounter.Instance;
        PlaySound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        var deliveryCounter = DeliveryCounter.Instance;
        PlaySound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume);
    }
    
    private void PlaySound(AudioClip[] audioClips, Vector3 position, float volumeMultiplier = 1f)
    {
        var audioClip = audioClips[Random.Range(0, audioClips.Length)];
        PlaySound(audioClip, position, volumeMultiplier * _volume);
    }

    public void PlayFootstepsSound(Vector3 position, float volumeMultiplier)
    {
        PlaySound(audioClipRefsSO.footstep, position, volumeMultiplier * _volume);
    }

    public void ChangeVolume()
    {
        _volume += .1f;
        
        if (_volume > 1.05)
            _volume = 0;
        
        PlayerPrefs.SetFloat(PlayerPrefsSoundEffectsVolume, _volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return _volume;
    }
}
