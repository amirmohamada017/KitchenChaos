using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    
    [SerializeField] private KitchenObjectListSO kitchenObjectsSO;
    [SerializeField] private List<Color> playerColors;

    public const int MaxPlayerAmount = 4;
    private NetworkList<PlayerData> _playerDataNetworkList;
    private const string PlayerPrefsPlayerName = "PlayerName";
    private string _playerName;
    
    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);

        _playerName = PlayerPrefs.GetString(PlayerPrefsPlayerName, "Player" + UnityEngine.Random.Range(100, 1000));
        
        _playerDataNetworkList = new NetworkList<PlayerData>();
        _playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }
    
    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (var i = 0; i < _playerDataNetworkList.Count; i++)
        {
            var playerData = _playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
                break;
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        _playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorIndex = GetFirstUnusedColorIndex(),
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(GetPlayerId());
    }

    private void NetworkManager_ConnectionApprovalCallback(
        NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
        NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (!SceneManager.GetActiveScene().name.Equals(Loader.Scene.CharacterSelectScene.ToString()))
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has started!";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MaxPlayerAmount)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Max player count reached!";
            return;
        }
        
        connectionApprovalResponse.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientDisconnectedCallback; 
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientDisconnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(GetPlayerId());
    }

    private string GetPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        var playerData = _playerDataNetworkList[playerDataIndex];
        playerData.playerName = playerName;
        _playerDataNetworkList[playerDataIndex] = playerData;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        var playerData = _playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        _playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        var kitchenObjectIndex = GetKitchenObjectSOIndex(kitchenObjectSO);
        SpawnKitchenObjectServerRpc(kitchenObjectIndex, kitchenObjectParent.GetNetworkObject());
    }


    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex,
        NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        var kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        
        var kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        
        var networkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
        
        var kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectReference.TryGet(out var kitchenObjectParentNetworkObject);
        var kitchenObjectParent = kitchenObjectParentNetworkObject.gameObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }
    
    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectsSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }
    
    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectsSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        ClearKitchenObjectParentClientRpc(kitchenObjectNetworkObjectReference);
        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        kitchenObject.ClearKitchenObjectOnParent();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorIndex)
    {
        return playerColors[colorIndex];
    }

    public PlayerData GetPlayerData()
    {
        var clientId = NetworkManager.Singleton.LocalClientId;
        return GetPlayerDataFromClientId(clientId);
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (var playerData in _playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }

        return default;
    }
    
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (var i = 0; i < _playerDataNetworkList.Count; i++)
        {
            if (_playerDataNetworkList[i].clientId == clientId)
                return i;
        }

        return -1;
    }

    public void ChangePlayerColor(int colorIndex)
    {
        ChangePlayerColorServerRpc(colorIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorIndex, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorIndex))
            return;
        
        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        var playerData = _playerDataNetworkList[playerDataIndex];
        playerData.colorIndex = colorIndex;
        _playerDataNetworkList[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorIndex)
    {
        foreach (var playerData in _playerDataNetworkList)
        {
            if (playerData.colorIndex == colorIndex)
                return false;
        }

        return true;
    }
    
    private int GetFirstUnusedColorIndex()
    {
        for (var i = 0; i < playerColors.Count; i++)
        {
            if (IsColorAvailable(i))
                return i;
        }
        
        return -1;
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    public string GetPlayerName()
    {
        return _playerName;
    }

    public void SetPlayerName(string playerName)
    {
        _playerName = playerName;
        
        PlayerPrefs.SetString(PlayerPrefsPlayerName, playerName);
    }
}
