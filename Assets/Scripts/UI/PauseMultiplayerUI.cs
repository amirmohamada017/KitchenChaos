using System;
using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnMultiPlayerGamePaused += KitchenGameManager_OnMultiPlayerGamePaused;
        KitchenGameManager.Instance.OnMultiPlayerGameUnPaused += KitchenGameManager_OnMultiPlayerGameUnPaused;
        
        Hide();
    }

    private void KitchenGameManager_OnMultiPlayerGameUnPaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenGameManager_OnMultiPlayerGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
