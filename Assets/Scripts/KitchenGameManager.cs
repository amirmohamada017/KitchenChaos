using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnMultiPlayerGamePaused;
    public event EventHandler OnMultiPlayerGameUnPaused;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnPaused;
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
    private NetworkVariable<bool> _isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> _playerReadyDictionary; 
    private Dictionary<ulong, bool> _playerPausedDictionary;
    private const float CountdownToStartTimerMax = 3f;
    private const float GamePlayingTimerMax = 90f;
    private bool _isLocalGamePaused;
    private bool _isLocalPlayerReady;

    private void Awake()
    {
        Instance = this;
        _playerReadyDictionary = new Dictionary<ulong, bool>();
        _playerPausedDictionary = new Dictionary<ulong, bool>();
        _isLocalGamePaused = false;
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
        _isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (_isGamePaused.Value)
        {
            Time.timeScale = 0f;
            
            OnMultiPlayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            
            OnMultiPlayerGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
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
        _isLocalGamePaused = !_isLocalGamePaused; 
        
        if (_isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        } else 
        {
            UnPauseGameServerRpc();
            OnLocalGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        
        TestGamePausedState();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        
        TestGamePausedState();
    }
    
    private void TestGamePausedState()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (_playerPausedDictionary.ContainsKey(clientId) && _playerPausedDictionary[clientId])
            {
                _isGamePaused.Value = true;
                return;
            }
        }

        _isGamePaused.Value = false;
    }

    public bool IsLocalPlayerReady()
    {
        return _isLocalPlayerReady;
    }
}
