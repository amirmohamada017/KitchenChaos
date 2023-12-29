using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    private const string PlayerPrefsBindings = "InputBindings";
    
    public static GameInput Instance { get; private set; }
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;

    public enum Binding
    {
        MoveUp,
        MoveUpArrow,
        MoveDown,
        MoveDownArrow,
        MoveRight,
        MoveRightArrow,
        MoveLeft,
        MoveLeftArrow,
        Interact,
        InteractAlt,
        Pause,
        MoveGamepad,
        InteractGamepad,
        InteractAltGamepad,
        PauseGamepad
    }
    
    private PlayerControls _playerControls;
    private void Awake()
    {
        Instance = this;
        
        _playerControls = new PlayerControls();
        
        if (PlayerPrefs.HasKey(PlayerPrefsBindings))
            _playerControls.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PlayerPrefsBindings));
        
        _playerControls.Enable();

        _playerControls.Player.Interact.performed += Interact_performed;
        _playerControls.Player.InteractAlternate.performed += InteractAlternate_performed;
        _playerControls.Player.Pause.performed += Pause_performed;

        
    }

    private void OnDestroy()
    {
        _playerControls.Player.Interact.performed -= Interact_performed;
        _playerControls.Player.InteractAlternate.performed -= InteractAlternate_performed;
        _playerControls.Player.Pause.performed -= Pause_performed;
        
        _playerControls.Dispose();
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

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Interact:
                return _playerControls.Player.Interact.bindings[0].ToDisplayString();
            case Binding.InteractAlt:
                return _playerControls.Player.InteractAlternate.bindings[0].ToDisplayString();
            case Binding.MoveUp:
                return _playerControls.Player.Move.bindings[1].ToDisplayString();
            case Binding.MoveDown:
                return _playerControls.Player.Move.bindings[2].ToDisplayString();
            case Binding.MoveLeft:
                return _playerControls.Player.Move.bindings[3].ToDisplayString();
            case Binding.MoveRight:
                return _playerControls.Player.Move.bindings[4].ToDisplayString();
            case Binding.MoveUpArrow:
                return _playerControls.Player.Move.bindings[6].ToDisplayString();
            case Binding.MoveDownArrow:
                return _playerControls.Player.Move.bindings[7].ToDisplayString();
            case Binding.MoveLeftArrow:
                return _playerControls.Player.Move.bindings[8].ToDisplayString();
            case Binding.MoveRightArrow:
                return _playerControls.Player.Move.bindings[9].ToDisplayString();
            case Binding.MoveGamepad:
                return _playerControls.Player.Move.bindings[10].ToDisplayString();
            case Binding.Pause:
                return "ECS";
            case Binding.InteractGamepad:
                return _playerControls.Player.Interact.bindings[1].ToDisplayString();
            case Binding.InteractAltGamepad:
                return _playerControls.Player.InteractAlternate.bindings[1].ToDisplayString();
            case Binding.PauseGamepad:
                return _playerControls.Player.Pause.bindings[1].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        
        _playerControls.Player.Disable();
        InputAction inputAction;
        int bindingIndex;

        switch (binding)
        {
            default:
            case Binding.MoveUp:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.MoveDown:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.MoveRight:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.MoveLeft:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.MoveUpArrow:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 6;
                break;
            case Binding.MoveDownArrow:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 7;
                break;
            case Binding.MoveRightArrow:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 8;
                break;
            case Binding.MoveLeftArrow:
                inputAction = _playerControls.Player.Move;
                bindingIndex = 9;
                break;
            case Binding.Interact:
                inputAction = _playerControls.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.InteractAlt:
                inputAction = _playerControls.Player.InteractAlternate;
                bindingIndex = 0;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex).OnComplete(callback =>
        {
            callback.Dispose();
            _playerControls.Player.Enable();
            onActionRebound();
            
            PlayerPrefs.SetString(PlayerPrefsBindings, _playerControls.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
            
            OnBindingRebind?.Invoke(this, EventArgs.Empty);
        }).Start();

    }
}
