using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    
    private PlayerControls _playerControls;
    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();

        _playerControls.Player.Interact.performed += Interact_performed;
        _playerControls.Player.InteractAlternate.performed += InteractAlternate_performed;
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }
    
    public Vector2 GetMovementVectorNormalized()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
}
