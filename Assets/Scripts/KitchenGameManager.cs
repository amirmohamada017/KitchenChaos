using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    
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
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.CountdownToStart:
                _timer -= Time.deltaTime;
                if (_timer < 0f) {
                    _state = State.GamePlaying;
                    _timer = GamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                _timer -= Time.deltaTime;
                if (_timer < 0f) {
                    _state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
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

    public bool IsCountdownToStartActive()
    {
        return _state == State.CountdownToStart;
    }

    public bool IsGameOver()
    {
        return _state == State.GameOver;
    }

    public float GetCountdownToStartTimer()
    {
        if (_state == State.CountdownToStart)
            return _timer;
        else
            return 0;
    }
}
