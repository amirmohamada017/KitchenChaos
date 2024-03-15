using Unity.Netcode;
using UnityEngine;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }
    
    [SerializeField] private KitchenObjectListSO kitchenObjectsSO;
    
    private void Awake()
    {
        Instance = this;
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
    
    private int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectsSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }
    
    private KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectsSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }
}
