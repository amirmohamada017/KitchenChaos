using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned; 
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess; 
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }
    [SerializeField] private RecipeListSO recipeListSO;
    
    private List<RecipeSO> _waitingRecipeSOList;
    private float _spawnRecipeTimer = 4f;
    private const float SpawnRecipeTimerMax = 4f;
    private const int WaitingRecipesMax = 4;
    private int _successfulRecipesAmount = 0;

    private void Awake()
    {
        Instance = this;
        _waitingRecipeSOList = new List<RecipeSO>();
    }
    
    private void Update()
    {
        if(!IsServer) return;
        
        _spawnRecipeTimer -= Time.deltaTime;
        if (!(_spawnRecipeTimer <= 0f)) return;
        _spawnRecipeTimer = SpawnRecipeTimerMax;

        if (_waitingRecipeSOList.Count >= WaitingRecipesMax || !KitchenGameManager.Instance.IsGamePlaying()) return;
        
        var waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSoList.Count);
        SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        var waitingRecipeSO = recipeListSO.recipeSoList[waitingRecipeSOIndex];
        _waitingRecipeSOList.Add(waitingRecipeSO);
        
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (var i = 0; i < _waitingRecipeSOList.Count; i++)
        {   
            var waitingRecipeSO = _waitingRecipeSOList[i];
        
            if (waitingRecipeSO.kitchenObjectSoList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                var plateContentMatchesRecipe = true;
                foreach (var recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSoList)
                {
                    var ingredientFound = false;
                    foreach (var plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }

                    if (!ingredientFound)
                        plateContentMatchesRecipe = false;
                }

                if (plateContentMatchesRecipe)
                {
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOIndex)
    {
        _successfulRecipesAmount++;
        _waitingRecipeSOList.Remove(_waitingRecipeSOList[waitingRecipeSOIndex]);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
    
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return _waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return _successfulRecipesAmount;
    }
}
