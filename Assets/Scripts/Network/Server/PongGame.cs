using System;
using System.Collections.Generic;
using UnityEngine;

public class PongGame
{
    private PongClient _p1;
    private PongClient _p2;
    private Dictionary<PongClient, Position> _position;

    private MessageFactory _factory;
    private float _speed = 6.11f;

    public PongGame(PongClient p1, PongClient p2, MessageFactory factory)
    {
        _p1 = p1;
        _p2 = p2;
        _factory = factory;
        _position = new Dictionary<PongClient, Position>()
        {
            { _p1, new Position()},
            { _p2, new Position()},
        };
        p1.OnMessageReceived += HandleMessage;
        p2.OnMessageReceived += HandleMessage;

        var timer = new System.Threading.Timer((e) =>
        {
            SendPositions();
        }, null, 1000, 5);
    }

    private void HandleMessage(PongClient sender, RawPongMessage msg)
    {

        Debug.Log($"Handle {msg.Command} {String.Join(",", msg.Args)} from player #{sender.ID}");

        // Change switch for better design
        switch (msg.Command)
        {
            case "MOVE":
                int mouvement = Convert.ToInt32(msg.Args[1].Trim());
                UpdatePosition(sender, mouvement);
                break;
        }
    }

    private void UpdatePosition(PongClient sender, int mouvement)
    {
        Position p = _position[sender];
        _position[sender] = p.SetMouvement(mouvement);
    }

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

    struct Position
    {
        public float OldPosition { get; }
        public float CurrentPosition { get; }
        public int Mouvement { get; }

        public Position(float oldPosition, float currentPosition, int mouvement)
        {
            OldPosition = oldPosition;
            CurrentPosition = currentPosition;
            Mouvement = mouvement;
        }

        public Position SetCurrent(float newCurrent) => new Position(OldPosition, newCurrent, Mouvement);
        public Position SetOld(float newOld) => new Position(newOld, CurrentPosition, Mouvement);
        public Position SetMouvement(int newMouvement) => new Position(OldPosition, CurrentPosition, newMouvement);
    }
}
