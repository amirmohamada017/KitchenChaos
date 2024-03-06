using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State _state;
    private const float CountdownToStartTimerMax = 1f;
    private const float GamePlayingTimerMax = 300f;
    private float _timer;
    private bool _isGamePause;

    private void Awake()
    {
        Instance = this;
        _isGamePause = false;
        _state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;

        _state = State.CountdownToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (_state != State.WaitingToStart) return;
        
        _state = State.CountdownToStart;
        _timer = CountdownToStartTimerMax;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        switch (_state)
        {
            default:
            case State.WaitingToStart:
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

    public void TogglePauseGame()
    {
        _isGamePause = !_isGamePause; 
        
        if (_isGamePause)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        } else 
        {
            Time.timeScale = 1f;
            OnGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
