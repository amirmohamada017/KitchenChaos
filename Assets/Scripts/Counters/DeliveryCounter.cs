using System;

public class DeliveryCounter : BaseCounter
{
    
    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out var plateKitchenObject))
        {
            DeliveryManager.Instance.DeliverRecipe(plateKitchenObject);
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
        }
    }
}
