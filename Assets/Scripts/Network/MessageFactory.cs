using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessageFactory
{
    public IPongMessage GetMoveMessage(int playerId, float delta)
    {
        return new MovePongMessage(playerId, delta);
    }

    public IPongMessage GetAssignMessage(int id)
    {
        return new AssignPongMessage(id);
    }

    public IPongMessage GetPaddlePositionMessage(int id, float position)
    {
        return new PaddlePositionMessage(id, position);
    }
}

public struct RawPongMessage
{
    private static char[] _splitChar = new char[] { '|' };
    public string Command { get; private set; }
    public string[] Args { get; private set; }

    public static RawPongMessage Parse(string msg)
    {
        string[] split = msg.Split(_splitChar, 2);
        (string cmd, string rawArgs) = (split[0], split[1]);
        string[] args = rawArgs.Split(_splitChar);

        return new RawPongMessage() { Command = cmd, Args = args };
    }

    public override string ToString()
    {
        return $"{Command}|{string.Join("|", Args)}";
    }
}

public interface IPongMessage
{
    string GetMessage();
}

public struct MovePongMessage : IPongMessage
{
    private float _delta;
    private int _playerId;

    public float Mouvement => _delta;
    public int PlayerId => _playerId;
    public MovePongMessage(int playerId, float delta)
    {
        _delta = delta;
        _playerId = playerId;
    }

    public string GetMessage()
    {
        return $"MOVE|{_playerId}|{_delta}";
    }
}

public struct AssignPongMessage : IPongMessage
{
    private int _id;
    public AssignPongMessage(int id)
    {
        _id = id;
    }

    public string GetMessage()
    {
        return $"ASSIGN|{_id}";
    }
}

public struct PaddlePositionMessage : IPongMessage
{
    private int _id;
    private float _paddlePosition;
    public PaddlePositionMessage(int playerId, float paddlePosition)
    {
        _id = playerId;
        _paddlePosition = paddlePosition;
    }

    public string GetMessage()
    {
        return $"POSITION|{_id}|{_paddlePosition}";
    }
}
