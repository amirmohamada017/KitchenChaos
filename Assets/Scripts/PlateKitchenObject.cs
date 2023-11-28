using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO KitchenObjectSO;
    }
    
    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSoList;
    
    private List<KitchenObjectSO> _kitchenObjectSoList;

    private void Awake()
    {
        _kitchenObjectSoList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (_kitchenObjectSoList.Contains(kitchenObjectSO) || !validKitchenObjectSoList.Contains(kitchenObjectSO))
            return false;
        
        _kitchenObjectSoList.Add(kitchenObjectSO); 
        
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            KitchenObjectSO = kitchenObjectSO
        });
        
        return true;
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return _kitchenObjectSoList;
    }
}
