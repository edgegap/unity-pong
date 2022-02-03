using System.Collections;
using System.Collections.Generic;
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
}

public interface IPongMessage
{
    string GetMessage();
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
}

public struct MovePongMessage : IPongMessage
{
    private float _delta;
    private int _playerId;
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
