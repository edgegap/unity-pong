using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    PaddleController controller;
    [SerializeField]
    PongServerCommunicator com;

    private int _playerId;
    // Start is called before the first frame update
    void Start()
    {
        com.OnPlayerAssignment += (id) => _playerId = (id % 2) + 1;
        com.OnMoveCommand += (id, move) =>
        {
            if (id == _playerId)
            {
                controller.Move(move);
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
    }
}
