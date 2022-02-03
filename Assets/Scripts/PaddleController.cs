using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField]
    Transform paddle;
    [SerializeField]
    float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float moveValue)
    {
        float delta = moveValue * Time.deltaTime;
        paddle.position = new Vector3(paddle.position.x, paddle.position.y + delta * speed);
    }
}
