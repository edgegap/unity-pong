using System;
using UnityEngine;

public class LocalController : MonoBehaviour
{
    [SerializeField]
    PaddleController controller;
    [SerializeField]
    PongServerCommunicator com;

    float _position = 0;

    private int _playerId;
    private float _lastMouvement = 0;
    // Start is called before the first frame update
    void Start()
    {
        com.OnPlayerAssignment += (id) => _playerId = id;
        com.OnPositionChanged += HandlePositionChanged;
    }

    // Update is called once per frame
    void Update()
    {
        float value = Input.GetAxisRaw("Vertical");
        if (value != _lastMouvement)
        {
            _lastMouvement = value;
            com.SendMovePaddle(_playerId, value);
        }

        controller.SetPaddlePosition(_position);
    }

    private void HandlePositionChanged(int id, float position)
    {
        if (id == _playerId)
        {
            _position = position;
        }
    }
}
