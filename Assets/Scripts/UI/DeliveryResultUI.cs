using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite successIconImage;
    [SerializeField] private Sprite failIconImage;
    [SerializeField] private Color successColor;
    [SerializeField] private Color failColor;
    [SerializeField] private TextMeshProUGUI messageText;

    private const string Popup = "Popup";
    private Animator _animator;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        
        gameObject.SetActive(false);
    }
    
    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
        _animator.SetTrigger(Popup);
        backgroundImage.color = failColor;
        iconImage.sprite = failIconImage;
        messageText.text = "DELIVERY\nFAILED";
    }
    
    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
        _animator.SetTrigger(Popup);
        backgroundImage.color = successColor;
        iconImage.sprite = successIconImage;
        messageText.text = "DELIVERY\nSUCCESS";
    }
}
