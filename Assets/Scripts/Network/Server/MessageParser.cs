using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRawMessageInterpreter
{
    event Action<PongClient, MovePongMessage> OnMoveMessage;
    event Action<PongClient, RawPongMessage> OnUnkownMessage;

    void Parse(PongClient sender, RawPongMessage msg);
}

public class RawMessageInterpreter : IRawMessageInterpreter
{
    public event Action<PongClient, MovePongMessage> OnMoveMessage;
    public event Action<PongClient, RawPongMessage> OnUnkownMessage;

    public void Parse(PongClient sender, RawPongMessage msg)
    {

        // Change switch for better design
        switch (msg.Command)
        {
            case "MOVE":
                int playerId = Convert.ToInt32(msg.Args[0].Trim()); 
                int mouvement = Convert.ToInt32(msg.Args[1].Trim());
                OnMoveMessage?.Invoke(sender, new MovePongMessage(playerId, mouvement));
                break;
            default:
                OnUnkownMessage?.Invoke(sender, msg);
                break;
        }
    }
}
