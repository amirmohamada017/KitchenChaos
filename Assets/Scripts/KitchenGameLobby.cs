using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby _joinedLobby;
    private float _listLobbiesTimer;

    private void Update()
    {
        HandlePeriodicListLobbies();
    }
    
    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeUnityAuthentication();
    }
    
    private bool IsLobbyHost()
    {
        return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            var initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());
            await UnityServices.InitializeAsync(initializationOptions);
            
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
    
    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 
                KitchenGameMultiplayer.MaxPlayerAmount, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });
            
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public async void JoinByCode(string lobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinById(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public Lobby GetLobby()
    {
        return _joinedLobby;
    }

    public async void DeleteLobby()
    {
        if (_joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);

                _joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    
    public async void LeaveLobby()
    {
        if (_joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                
                _joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    
    public async void KickPlayer(string clientId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, clientId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            var queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
        
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
        
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private void HandlePeriodicListLobbies()
    {
        if (_joinedLobby == null && AuthenticationService.Instance.IsSignedIn)
        {
            _listLobbiesTimer -= Time.deltaTime;
            if (_listLobbiesTimer < 0f)
            {
                const float listLobbiesTimerMax = 3f;
                _listLobbiesTimer += listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }
}
