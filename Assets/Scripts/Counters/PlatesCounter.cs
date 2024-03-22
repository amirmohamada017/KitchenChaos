using System;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;
    
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    
    private float _spawnPlateTimer;
    private const float SpawnPlateTimerMax = 4f;
    private int _platesSpawnedAmount;
    private const int PlatesSpawnedAmountMax = 4;

    private void Update()
    {
        if (!IsServer) return;
        
        _spawnPlateTimer += Time.deltaTime;
        if (_spawnPlateTimer > SpawnPlateTimerMax)
        {
            _spawnPlateTimer = 0f;
            if (_platesSpawnedAmount < PlatesSpawnedAmountMax && KitchenGameManager.Instance.IsGamePlaying())
                SpawnPlateServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        _platesSpawnedAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }
    
    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject() && HasPlate())
        {
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            InteractLogicServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        _platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }

    private bool HasPlate()
    {
        return _platesSpawnedAmount > 0;
    }
}
