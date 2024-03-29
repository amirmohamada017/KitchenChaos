using UnityEngine;
using UnityEngine.UI;

public class TestingChracterSelectUI : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    
    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }
}
