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
    private const float GamePlayingTimerMax = 20f;
    private float _timer;
    private bool _isGamePause = false;

    private void Awake()
    {
        Instance = this;
        _state = State.WaitingToStart;
        _timer = WaitingToStartTimerMax;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
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
        }
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
        return 0;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return _state switch
        {
            State.GameOver => 1,
            State.GamePlaying => 1 - (_timer / GamePlayingTimerMax),
            State.CountdownToStart => 0,
            State.WaitingToStart => 0,
            _ => 1
        };
    }

    private void TogglePauseGame()
    {
        Time.timeScale = _isGamePause ? 0f : 1f;
        _isGamePause = !_isGamePause;
    }
}
