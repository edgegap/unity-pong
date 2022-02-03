using UnityEngine;

public class LocalController : MonoBehaviour
{
    [SerializeField]
    PaddleController controller;
    [SerializeField]
    PongServerCommunicator com;

    private int _playerId;
    // Start is called before the first frame update
    void Start()
    {
        com.OnPlayerAssignment += (id) => _playerId = id;
    }

    // Update is called once per frame
    void Update()
    {
        float value = Input.GetAxisRaw("Vertical");
        if (value != 0)
        {
            com.SendMovePaddle(_playerId, value);
            controller.Move(value);
        }
    }
}
