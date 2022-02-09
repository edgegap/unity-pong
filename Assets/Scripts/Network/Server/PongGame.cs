using System;
using System.Collections.Generic;
using UnityEngine;

struct Position
{
    public float OldPosition { get; }
    public float CurrentPosition { get; }
    public float Mouvement { get; }

    public Position(float oldPosition, float currentPosition, float mouvement)
    {
        OldPosition = oldPosition;
        CurrentPosition = currentPosition;
        Mouvement = mouvement;
    }

    public Position SetCurrent(float newCurrent) => new Position(OldPosition, newCurrent, Mouvement);
    public Position SetOld(float newOld) => new Position(newOld, CurrentPosition, Mouvement);
    public Position SetMouvement(float newMouvement) => new Position(OldPosition, CurrentPosition, newMouvement);
}

public class PongGame
{
    public event Action OnGameOver;

    private PongClient _p1;
    private PongClient _p2;
    private IRawMessageInterpreter _interpreter;
    private Dictionary<PongClient, Position> _position;

    private MessageFactory _factory;
    private float _speed = 6.11f;

    public PongGame(MessageFactory factory, IRawMessageInterpreter interpreter)
    {
        _factory = factory;
        _position = new Dictionary<PongClient, Position>(2);
        _interpreter = interpreter;

        // Handle interpreted messages
        _interpreter.OnMoveMessage += HandleMoveMessage;
        _interpreter.OnUnkownMessage += HandleUnkownMessage;

    }

    public void AddPlayer(PongClient player)
    {
        if (_p1 == null)
        {
            _p1 = player;
        }
        else if (_p2 == null)
        {
            _p2 = player;
        }
        else
        {
            throw new InvalidOperationException("Game is full");
        }

        // Handle players message
        player.OnMessageReceived += _interpreter.Parse;

        // Handle connection lost
        player.OnConnectionLost += HandleConenctionLost;

        _position[player] = new Position();
    }

    public void Start()
    {
        // Sending position data to players each X ms
        var timer = new System.Threading.Timer((e) =>
        {
            SendPositions();
        }, null, 0, 1);
    }


    /// <summary>
    /// Handle move message from players. This will update their mouvement based on the message
    /// </summary>
    private void HandleConenctionLost(PongClient sender)
    {
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// Handle move message from players. This will update their mouvement based on the message
    /// </summary>
    private void HandleMoveMessage(PongClient sender, MovePongMessage msg)
    {
        Debug.Log($"Handle ({msg.GetMessage()}) from player #{sender.ID}");
        Position p = _position[sender];
        _position[sender] = p.SetMouvement((int)msg.Mouvement);
    }

    /// <summary>
    /// Handle unkown message from players. This will log the message
    /// </summary>
    private void HandleUnkownMessage(PongClient sender, RawPongMessage msg)
    {
        Debug.Log($"Received unkown message ({msg.ToString()}) from player #{sender.ID}");
    }

    /// <summary>
    /// Update the players position based on their mouvement and current position
    /// </summary>
    public void UpdatePlayersPosition()
    {
        UpdatePlayerPosition(_p1);
        UpdatePlayerPosition(_p2);
    }

    private void UpdatePlayerPosition(PongClient player)
    {
        Position p = _position[player];
        float delta = p.Mouvement * Time.deltaTime;
        _position[player] = _position[player].SetCurrent(_position[player].CurrentPosition + delta * _speed);
    }

    /// <summary>
    /// Send each player's position to every players
    /// </summary>
    private void SendPositions()
    {
        SendPosition(_p1);
        SendPosition(_p2);
    }

    private void SendPosition(PongClient player)
    {
        Position position = _position[player];

        if (Math.Abs(position.CurrentPosition - position.OldPosition) > 0.005)
        {
            _position[player] = position.SetOld(position.CurrentPosition);
            IPongMessage msg = _factory.GetPaddlePositionMessage(player.ID, position.CurrentPosition);
            _p1.SendMessage(msg);
            _p2.SendMessage(msg);
        }
    }
}
