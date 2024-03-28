using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> _timer = new NetworkVariable<float>(0f);
    private Dictionary<ulong, bool> _playerReadyDictionary; 
    private const float CountdownToStartTimerMax = 3f;
    private const float GamePlayingTimerMax = 90f;
    private bool _isGamePause;
    private bool _isLocalPlayerReady;

    private void Awake()
    {
        Instance = this;
        _playerReadyDictionary = new Dictionary<ulong, bool>();
        _isGamePause = false;
        _isLocalPlayerReady = false;
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    public override void OnNetworkSpawn()
    {
        _state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (_state.Value != State.WaitingToStart) return;

        _isLocalPlayerReady = true;
        
        OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
        
        SetPlayerReadyServerRpc();
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        var allClientsReady = true;
        foreach (ulong clientId  in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            _state.Value = State.CountdownToStart;
            _timer.Value = CountdownToStartTimerMax;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer) return;
        
        switch (_state.Value)
        {
            default:
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _timer.Value -= Time.deltaTime;
                if (_timer.Value < 0f) {
                    _state.Value = State.GamePlaying;
                    _timer.Value = GamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                _timer.Value -= Time.deltaTime;
                if (_timer.Value < 0f) {
                    _state.Value = State.GameOver;
                }              
                break;
            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return _state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return _state.Value == State.CountdownToStart;
    }

    public bool IsGameOver()
    {
        return _state.Value == State.GameOver;
    }

    public float GetCountdownToStartTimer()
    {
        if (_state.Value == State.CountdownToStart)
            return _timer.Value;
        return 0;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return _state.Value switch
        {
            State.GameOver => 1,
            State.GamePlaying => 1 - (_timer.Value / GamePlayingTimerMax),
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

    public bool IsLocalPlayerReady()
    {
        return _isLocalPlayerReady;
    }
}
