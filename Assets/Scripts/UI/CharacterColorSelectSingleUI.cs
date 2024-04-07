using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorIndex;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.ChangePlayerColor(colorIndex);
        });
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged +=
            KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        
        image.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorIndex);

        UpdateIsSelected();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        selectedGameObject.SetActive(KitchenGameMultiplayer.Instance.GetPlayerData().colorIndex == colorIndex);
    }
    
    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -=
            KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
