using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float moveSpeed = 7f;
    private bool _isWalking;
    
    private void Update()
    {
        var inputVector = gameInput.GetMovementVectorNormalized();
        var moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        transform.position += moveDir * (Time.deltaTime * moveSpeed);

        var rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);

        _isWalking = moveDir != Vector3.zero;
    }

    public bool Is_Walking()
    {
        return _isWalking;
    }
}
