using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W))
            inputVector.y++;
        if (Input.GetKey(KeyCode.A))
            inputVector.x--;
        if (Input.GetKey(KeyCode.S))
            inputVector.y--;
        if (Input.GetKey(KeyCode.D))
            inputVector.x++;

        return inputVector.normalized;
    }
}
