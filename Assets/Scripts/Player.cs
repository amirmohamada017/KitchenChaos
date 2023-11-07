using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float moveSpeed = 7f;
    private bool _isWalking;
    
    private void Update()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        var moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        var moveDirX = new Vector3(inputVector.x, 0, 0);
        var moveDirZ = new Vector3(0, 0, inputVector.y);
        
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

    public bool Is_Walking()
    {
        return _isWalking;
    }
}
