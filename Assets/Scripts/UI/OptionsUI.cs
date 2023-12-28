using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { private set; get; }
    
    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveUpArrowButton;
    [SerializeField] private Button moveDownArrowButton;
    [SerializeField] private Button moveRightArrowButton;
    [SerializeField] private Button moveLeftArrowButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button interactAlternateButton;
    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveUpArrowText;
    [SerializeField] private TextMeshProUGUI moveDownArrowText;
    [SerializeField] private TextMeshProUGUI moveRightArrowText;
    [SerializeField] private TextMeshProUGUI moveLeftArrowText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAlternateText;
    [SerializeField] private Transform pressToRebindingTransform;

    private Action _onCloseButtonAction;

    private void Awake()
    {
        Instance = this;
        
        soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        
        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        
        closeButton.onClick.AddListener(() =>
        {
            Hide();
            _onCloseButtonAction();
        });
        
        moveUpButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveUp));
        moveUpArrowButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveUpArrow));
        moveDownButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveDown));
        moveDownArrowButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveDownArrow));
        moveRightButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveRight));
        moveRightArrowButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveRightArrow));
        moveLeftButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveLeft));
        moveLeftArrowButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveLeftArrow));
        interactButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.Interact));
        interactAlternateButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.InteractAlt));
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnGameUnPaused += KitchenGameManager_OnGameUnPaused;
        UpdateVisual();
        HidePressToRebindKey();
        Hide();
    }

    private void KitchenGameManager_OnGameUnPaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);
        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDown);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft);
        moveUpArrowText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUpArrow);
        moveDownArrowText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDownArrow);
        moveRightArrowText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRightArrow);
        moveLeftArrowText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeftArrow);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlt);
    }

    public void Show(Action onCloseButtonAction)
    {
        this._onCloseButtonAction = onCloseButtonAction;
        
        gameObject.SetActive(true);
        soundEffectsButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowPressToRebindKey()
    {
        pressToRebindingTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey()
    {
        pressToRebindingTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () =>
        {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }
}
