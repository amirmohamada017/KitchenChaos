using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject())
            player.GetKitchenObject().SetKitchenObjectParent(this);
        else if (HasKitchenObject() && !player.HasKitchenObject())
            GetKitchenObject().SetKitchenObjectParent(player);
        else if (HasKitchenObject() && player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out var plateKitchenObject))
            {
                if (plateKitchenObject!.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
            }
            else if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
            {
                if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                    KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
            }
        }
    }
}
