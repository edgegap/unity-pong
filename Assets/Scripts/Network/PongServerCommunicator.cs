using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongServerCommunicator : MonoBehaviour
{
    public event Action<int> OnPlayerAssignment;
    public event Action<int, float> OnMoveCommand;

    [SerializeField]
    ServerCommunicator _com;
    MessageFactory _messageFactory = new MessageFactory();

    // Start is called before the first frame update
    void Start()
    {
        _com.OnMessageReceived += ParseMessage;
    }

    // Update is called once per frame
    void Update() { }

    public void SendMovePaddle(int playerId, float delta)
    {
        IPongMessage msg = _messageFactory.GetMoveMessage(playerId, delta);
        _com.SendMessageServer(msg.GetMessage());
    }

    private void ParseMessage(string msg)
    {
        RawPongMessage pongMsg = RawPongMessage.Parse(msg);

        // TODO Change switch for better design
        switch (pongMsg.Command)
        {
            case "ASSIGN":
                OnPlayerAssignment?.Invoke(Convert.ToInt32(pongMsg.Args[0]));
                break;
            case "MOVE":
                OnMoveCommand?.Invoke(Convert.ToInt32(pongMsg.Args[0]),Convert.ToInt32(pongMsg.Args[1]));
                break;
        }
    }
}
