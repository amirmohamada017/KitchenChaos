using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectSOToGameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }
    
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSOToGameObject> kitchenObjectSOToGameObjects;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;

        foreach (var kitchenObjectSOToGameObject in kitchenObjectSOToGameObjects)
        {
            kitchenObjectSOToGameObject.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (var kitchenObjectSOToGameObject in kitchenObjectSOToGameObjects)
        {
            if (kitchenObjectSOToGameObject.kitchenObjectSO == e.KitchenObjectSO)
                kitchenObjectSOToGameObject.gameObject.SetActive(true);
            
        }
    }
}