using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned; 
    public event EventHandler OnRecipeCompleted; 

    public static DeliveryManager Instance { get; private set; }
    [SerializeField] private RecipeListSO recipeListSO;
    
    private List<RecipeSO> _waitingRecipeSOList;
    private float _spawnRecipeTimer;
    private const float SpawnRecipeTimerMax = 4f;
    private const int WaitingRecipesMax = 4;

    private void Awake()
    {
        Instance = this;
        _waitingRecipeSOList = new List<RecipeSO>();
    }
    
    private void Update()
    {
        _spawnRecipeTimer -= Time.deltaTime;
        if (!(_spawnRecipeTimer <= 0f)) return;
        _spawnRecipeTimer = SpawnRecipeTimerMax;

        if (_waitingRecipeSOList.Count >= WaitingRecipesMax) return;
        var waitingRecipeSO = recipeListSO.recipeSoList[UnityEngine.Random.Range(0, recipeListSO.recipeSoList.Count)];
        _waitingRecipeSOList.Add(waitingRecipeSO);
        
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        foreach (var waitingRecipeSO in _waitingRecipeSOList)
        {
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
                    _waitingRecipeSOList.Remove(waitingRecipeSO);
                    return;
                }
            }
        }
    }
    
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return _waitingRecipeSOList;
    }
}
