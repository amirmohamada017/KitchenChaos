using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;
    private PlayerControls _playerControls;
    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();

        _playerControls.Player.Interact.performed += Interact_performed;
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
}
