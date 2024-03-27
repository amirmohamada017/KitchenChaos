using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }
    
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;


    private NetworkVariable<State> _state = new NetworkVariable<State>(State.Idle);
    private FryingRecipeSO _fryingRecipeSO;
    private BurningRecipeSO _burningRecipeSO;
    private NetworkVariable<float> _fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> _burningTimer = new NetworkVariable<float>(0f);
    

    public override void OnNetworkSpawn()
    {
        _fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        _burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        _state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = _state.Value
        });

        if (_state.Value is State.Idle or State.Burned)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                ProgressNormalized = 0f
            });
        }
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        var burningTimerMax = _burningRecipeSO != null ? _burningRecipeSO.burningTimeMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            ProgressNormalized = _burningTimer.Value / burningTimerMax
        });
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        var fryingTimerMax = _fryingRecipeSO != null ? _fryingRecipeSO.fryingTimeMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            ProgressNormalized = _fryingTimer.Value / fryingTimerMax
        });
    }

    private void Update()
    {
        if (!IsServer) return;
        
        switch (_state.Value)
        {
            case State.Idle:
                break;
            case State.Frying:
                _fryingTimer.Value += Time.deltaTime;
                
                if (_fryingTimer.Value < _fryingRecipeSO.fryingTimeMax) break;
                
                _fryingTimer.Value = 0;
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
                KitchenObject.SpawnKitchenObject(_fryingRecipeSO.output, this);
                _burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                _state.Value = State.Fried;
                _burningTimer.Value = 0f;
                SetBurningRecipeSOClientRpc(
                    KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));
                break;
            
            case State.Fried:
                _burningTimer.Value += Time.deltaTime;
                
                if (_burningTimer.Value < _burningRecipeSO.burningTimeMax) break;
                
                _burningTimer.Value = 0;
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
                KitchenObject.SpawnKitchenObject(_burningRecipeSO.output, this);
                _state.Value = State.Burned;
                
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    ProgressNormalized = 0f
                });
                
                break;
            
            case State.Burned:
                break;
        }
    }

    public override void Interact(Player player)
    {
        if (HasKitchenObject())
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                SetServerIdleServerRpc();
            }
            else
            {
                if (!player.GetKitchenObject().TryGetPlate(out var plateKitchenObject)) 
                    return;
                
                if (!plateKitchenObject!.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) 
                    return;
                
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    
                SetServerIdleServerRpc();
                _fryingTimer.Value = 0f;
            }
        }
        else if (player.HasKitchenObject() && !HasKitchenObject() && HasRecipeWithInput(
                     player.GetKitchenObject().GetKitchenObjectSO()))
        {
            var kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectParent(this);

            InteractLogicPlaceObjectOnCounterServerRpc(
                KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetServerIdleServerRpc()
    {
        _state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        _fryingTimer.Value = 0f;
        _state.Value = State.Frying;
        
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }
    
    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        var kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        _fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }
    
    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        var kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        _burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);    
    }
    
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        return GetFryingRecipeSOWithInput(inputKitchenObjectSO) != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        var fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return (fryingRecipeSO != null) ? fryingRecipeSO.output : null;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (var fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
                return fryingRecipeSO;
        }

        return null;
    }
    
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (var burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
                return burningRecipeSO;
        }

        return null;
    }
}
