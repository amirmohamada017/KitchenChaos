using UnityEngine;

public class GameInput : MonoBehaviour
{
    private PlayerControls _playerControls;
    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
}
