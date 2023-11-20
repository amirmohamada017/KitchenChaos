using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cutKitchenObjectSOArray;
    
    public override void Interact(Player player)
    {
        if (HasKitchenObject() && !player.HasKitchenObject())
            GetKitchenObject().SetKitchenObjectParent(player);
        else if (player.HasKitchenObject() && !HasKitchenObject() && HasRecipeWithInput(
                     player.GetKitchenObject().GetKitchenObjectSO()))
            player.GetKitchenObject().SetKitchenObjectParent(this);
    }

    public override void InteractAlternate(Player player)
    {
        if (!HasKitchenObject()) return;

        var cutKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
        if (cutKitchenObjectSO == null) return;
        
        GetKitchenObject().DestroySelf();
        KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (var cuttingRecipeSO in cutKitchenObjectSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
                return true;
        }

        return false;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (var cuttingRecipeSO in cutKitchenObjectSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
                return cuttingRecipeSO.output;
        }

        return null;
    }
}
