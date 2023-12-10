using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player _player;
    private float _footstepTimer;
    private const float FootstepTimerMax = .1f;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer < 0f)
        {
            _footstepTimer = FootstepTimerMax;

            if (_player.Is_Walking())
            {
                const float volume = 1f;
                SoundManager.Instance.PlayFootstepsSound(_player.transform.position, volume);
            }
        }
    }
}
