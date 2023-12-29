using System;
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
        _spawnPlateTimer += Time.deltaTime;
        if (_spawnPlateTimer > SpawnPlateTimerMax && KitchenGameManager.Instance.IsGamePlaying())
        {
            _spawnPlateTimer = 0f;
            if (_platesSpawnedAmount < PlatesSpawnedAmountMax)
            {
                _platesSpawnedAmount++;
                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject() && HasPlate())
        {
            _platesSpawnedAmount--;
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            OnPlateRemoved?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool HasPlate()
    {
        return _platesSpawnedAmount > 0;
    }
}
