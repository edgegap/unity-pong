using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField]
    Transform paddle;

    public void SetPaddlePosition(float position)
    {
        paddle.position = Vector3.Lerp(
            paddle.position,
            new Vector3(paddle.position.x, position), 
            Time.deltaTime * 20
        );
    }
}
