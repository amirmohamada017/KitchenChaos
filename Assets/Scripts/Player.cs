using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { private set; get; }
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public ClearCounter selectedCounter;
    }
    
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    
    private bool _isWalking;
    private Vector3 _lastInteractDir;
    private ClearCounter _selectedCounter;

    private void Awake()
    {
        if (Instance != null)
            Debug.LogError("there is more than one player instance!");
        
        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (_selectedCounter != null)
            _selectedCounter.Interact();
    }

    private void Update()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        var moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        HandleMovement(moveDir);
        HandleInteractions(moveDir);
    }

    private void HandleMovement(Vector3 moveDir)
    {
        var moveDirX = new Vector3(moveDir.x, 0, 0);
        var moveDirZ = new Vector3(0, 0, moveDir.z);
        
        float moveDistance = moveSpeed * Time.deltaTime;
        float playerHeight = 2f;
        float playerRadius = .7f;
        
        bool canMoveX = !Physics.CapsuleCast(transform.position,
            (transform.position + Vector3.up * playerHeight), playerRadius, moveDirX, moveDistance);
        bool canMoveZ = !Physics.CapsuleCast(transform.position,
            (transform.position + Vector3.up * playerHeight), playerRadius, moveDirZ, moveDistance);
        
        if (canMoveX)
            transform.position += moveDirX * moveDistance;
        if (canMoveZ)
            transform.position += moveDirZ * moveDistance;
        
        var rotateSpeed = 10f;
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
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                if (clearCounter != null && clearCounter != _selectedCounter)
                    SetSelectedCounter(clearCounter);
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

    private void SetSelectedCounter(ClearCounter selectedCounter)
    {
        _selectedCounter = selectedCounter;
        
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = _selectedCounter
        });
    }
}
