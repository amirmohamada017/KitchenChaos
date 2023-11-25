using System;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject())
            player.GetKitchenObject().SetKitchenObjectParent(this);
        else if (!player.HasKitchenObject() && HasKitchenObject())
            GetKitchenObject().SetKitchenObjectParent(player);
    }
}
