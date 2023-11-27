using System;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State State;
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


    private State _state;
    private FryingRecipeSO _fryingRecipeSO;
    private BurningRecipeSO _burningRecipeSO;
    private float _fryingTimer;
    private float _burningTimer;


    private void Start()
    {
        _state = State.Idle;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:
                break;
            case State.Frying:
                _fryingTimer += Time.deltaTime;
                
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    ProgressNormalized = _fryingTimer / _fryingRecipeSO.fryingTimeMax
                });
                
                if (_fryingTimer < _fryingRecipeSO.fryingTimeMax) break;
                _fryingTimer = 0;
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(_fryingRecipeSO.output, this);
                _burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                _state = State.Fried;
                _burningTimer = 0f;
                _burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    State = _state
                });
                break;
            
            case State.Fried:
                _burningTimer += Time.deltaTime;
                
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    ProgressNormalized = _burningTimer / _burningRecipeSO.burningTimeMax
                });
                
                if (_burningTimer < _burningRecipeSO.burningTimeMax) break;
                _burningTimer = 0;
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(_burningRecipeSO.output, this);
                _state = State.Burned;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    State = _state
                });
                
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
                _state = State.Idle;

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    ProgressNormalized = 0f
                });

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    State = _state
                });
            }
            else
            {
                if (!player.GetKitchenObject().TryGetPlate(out var plateKitchenObject)) 
                    return;
                
                if (!plateKitchenObject!.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) 
                    return;
                
                GetKitchenObject().DestroySelf();
                    
                _state = State.Idle;
                _fryingTimer = 0f;
            
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    State = _state
                });
            
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    ProgressNormalized = _fryingTimer / _fryingRecipeSO.fryingTimeMax
                });
            }
        }
        else if (player.HasKitchenObject() && !HasKitchenObject() && HasRecipeWithInput(
                     player.GetKitchenObject().GetKitchenObjectSO()))
        {
            player.GetKitchenObject().SetKitchenObjectParent(this);
            _fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            _state = State.Frying;
            _fryingTimer = 0f;
            
            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
            {
                State = _state
            });
            
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                ProgressNormalized = _fryingTimer / _fryingRecipeSO.fryingTimeMax
            });
        }
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