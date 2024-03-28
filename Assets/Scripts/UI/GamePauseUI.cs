using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    public static GamePauseUI Instance { private set; get; }
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;

    private void Awake()
    {
        Instance = this;
        
        resumeButton.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGame();
        });
        
        optionsButton.onClick.AddListener(() =>
        {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
        
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        Time.timeScale = 1f;
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGamePaused += KitchenGameManagerOnLocalGamePaused;
        KitchenGameManager.Instance.OnLocalGameUnPaused += KitchenGameManagerOnLocalGameUnPaused;
        
        Hide();
    }

    private void KitchenGameManagerOnLocalGameUnPaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenGameManagerOnLocalGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        resumeButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
