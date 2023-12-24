using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State _state;
    private const float WaitingToStartTimerMax = 1f;
    private const float CountdownToStartTimerMax = 3f;
    private const float GamePlayingTimerMax = 10f;
    private float _timer;

    private void Awake()
    {
        Instance = this;
        _state = State.WaitingToStart;
        _timer = WaitingToStartTimerMax;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.WaitingToStart:
                _timer -= Time.deltaTime;
                if (_timer < 0f)
                {
                    _state = State.CountdownToStart;
                    _timer = CountdownToStartTimerMax;
                }
                break;
            case State.CountdownToStart:
                _timer -= Time.deltaTime;
                if (_timer < 0f) {
                    _state = State.GamePlaying;
                    _timer = GamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                _timer -= Time.deltaTime;
                if (_timer < 0f) {
                    _state = State.GameOver;
                }              
                break;
            case State.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Debug.Log(_state);
    }

    public bool IsGamePlaying()
    {
        return _state == State.GamePlaying;
    }
}
