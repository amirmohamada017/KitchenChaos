using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    
    public static GameInput Instance { get; private set; }
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    
    private PlayerControls _playerControls;
    private void Awake()
    {
        Instance = this;
        
        _playerControls = new PlayerControls();
        _playerControls.Enable();

        _playerControls.Player.Interact.performed += Interact_performed;
        _playerControls.Player.InteractAlternate.performed += InteractAlternate_performed;
        _playerControls.Player.Pause.performed += Pause_performed;
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }
    
    public Vector2 GetMovementVectorNormalized()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
}
