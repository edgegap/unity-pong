using UnityEngine;

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    PaddleController controller;
    [SerializeField]
    PongServerCommunicator com;

    float _position = 0;

    private int _playerId;
    // Start is called before the first frame update
    void Start()
    {
        com.OnPlayerAssignment += (id) => _playerId = (id % 2) + 1;
        com.OnPositionChanged += (id, pos) =>
        {
            Debug.Log($"Handle position changed for player #{id} -> {pos}");
            if (id == _playerId)
            {
                _position = pos;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        controller.SetPaddlePosition(_position);
    }
}
