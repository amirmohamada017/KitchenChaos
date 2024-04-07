using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static Player LocalInstance { private set; get; }
    
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething;

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositions;
    [SerializeField] private PlayerVisual playerVisual;
    
    private bool _isWalking;
    private Vector3 _lastInteractDir;
    private BaseCounter _selectedCounter;
    private KitchenObject _kitchenObject;
    
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    
    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        var playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorIndex));
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalInstance = this;

        transform.position =
            spawnPositions[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasNetworkObject)
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        
        if (_selectedCounter != null)
            _selectedCounter.Interact(this);
    }

    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        
        if (_selectedCounter != null)
            _selectedCounter.InteractAlternate(this);
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        var inputVector = GameInput.Instance.GetMovementVectorNormalized();
        var moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        
        HandleMovement(moveDir);
        HandleInteractions(moveDir);
    }

    private void HandleMovement(Vector3 moveDir)
    {
        var moveDirX = new Vector3(moveDir.x, 0, 0);
        var moveDirZ = new Vector3(0, 0, moveDir.z);
        
        var moveDistance = moveSpeed * Time.deltaTime;
        const float playerHeight = 2f;
        const float playerRadius = .7f;

        var canMoveX = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, 
            moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask);
        var canMoveZ = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, 
            moveDirZ, Quaternion.identity, moveDistance, collisionsLayerMask);
        
        if (canMoveX)
            transform.position += moveDirX * moveDistance;
        if (canMoveZ)
            transform.position += moveDirZ * moveDistance;
        
        const float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);

        _isWalking = moveDir != Vector3.zero;
    }

    private void HandleInteractions(Vector3 moveDir)
    {
        if (moveDir != Vector3.zero)
            _lastInteractDir = moveDir;
        
        var interactDistance = 2f;
        if (Physics.Raycast(transform.position, _lastInteractDir, out RaycastHit raycastHit,
                interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != null && baseCounter != _selectedCounter)
                    SetSelectedCounter(baseCounter);
            }
            else
                SetSelectedCounter(null);
        }
        else
            SetSelectedCounter(null);
    }

    public bool Is_Walking()
    {
        return _isWalking;
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        _selectedCounter = selectedCounter;
        
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = _selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return _kitchenObject;
    }

    public void ClearKitchenObject()
    {
        _kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return _kitchenObject != null;
    }
    
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
