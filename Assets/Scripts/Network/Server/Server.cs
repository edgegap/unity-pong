using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class PongClient
{
    public event Action<PongClient, RawPongMessage> OnMessageReceived;

    private MessageFactory _factory;
    private int _id;
    private TcpClient _client;
    private StreamWriter _writer;
    private StreamReader _reader;

    public int ID { get => _id; }

    public PongClient(TcpClient client, int id, MessageFactory factory)
    {
        _factory = factory;
        _id = id;
        _client = client;
        _writer = new StreamWriter(_client.GetStream());
        _reader = new StreamReader(_client.GetStream());
        Assign();
    }

    public Thread Start()
    {
        Thread thread = new Thread(Listen);
        thread.Start();
        return thread;
    }

    private void Listen()
    {
        Debug.Log($"Player #{_id} start listening");
        // Buffer for reading data
        byte[] bytes = new byte[256];
        NetworkStream stream = _client.GetStream();
        int i;

        try
        {
            // Loop to receive all the data sent by the client.
            while (true)
            {
                // Translate data bytes to a ASCII string.
                string data = _reader.ReadLine();
                data = data.Trim();
                Debug.Log($"Receiced ({data}) from player #{_id}");
                OnMessageReceived?.Invoke(this, RawPongMessage.Parse(data));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Debug.Log($"Player #{_id} stop listening");


        // Shutdown and end connection
        _client.Close();
    }

    public void SendMessage(IPongMessage msg)
    {
        string s = msg.GetMessage();
        Debug.Log($"Sending ({s}) to player #{_id}");
        _writer.WriteLine(s);
        _writer.Flush();
    }

    private void Assign()
    {
        IPongMessage msg = _factory.GetAssignMessage(_id);
        _writer.WriteLine(msg.GetMessage());
        _writer.Flush();
    }
}

public class PongGame
{
    private PongClient _p1;
    private PongClient _p2;
    private MessageFactory _factory;

    public PongGame(PongClient p1, PongClient p2, MessageFactory factory)
    {
        _p1 = p1;
        _p2 = p2;
        _factory = factory;
        p1.OnMessageReceived += HandleMessage;
        p2.OnMessageReceived += HandleMessage;
    }

    private void HandleMessage(PongClient sender, RawPongMessage msg)
    {
        // Change switch for better design
        Debug.Log($"Handle {msg.Command} {String.Join(",", msg.Args)}");
        
        switch (msg.Command)
        {
            case "MOVE":
                foreach (var c in msg.Args[1])
                {
                    Debug.Log($"c: {c}");

                }
                Debug.Log($"Len: {msg.Args[1].Length}, Value: {msg.Args[1]}");
                GetOtherPlayer(sender).SendMessage(_factory.GetMoveMessage(sender.ID, Convert.ToInt32(msg.Args[1].Trim())));
                break;
        }
    }

    private PongClient GetOtherPlayer(PongClient client) { 
        return client == _p1 ? _p2 : _p1;
    }
}

public class Server : MonoBehaviour
{
    private TcpListener _tcpListener;
    private int _port = 6969;
    private IPAddress _ip = IPAddress.Parse("0.0.0.0");

    // Start is called before the first frame update
    void Start()
    {
        MessageFactory messageFactory = new MessageFactory();
        System.Random r = new System.Random();
        List<Thread> threads = new List<Thread>(2);
        PongClient[] players = new PongClient[2];

        try
        {
            int playerCount = 0;
            _tcpListener = new TcpListener(_ip, _port);
            _tcpListener.Start();

            while (playerCount != 2)
            {
                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                int playerId = playerCount + 1;
                TcpClient client = _tcpListener.AcceptTcpClient();
                Debug.Log($"Player #{playerId} connected!");

                PongClient pongClient = new PongClient(client, playerId, messageFactory);
                players[playerCount] = pongClient;

                playerCount++;

                threads.Add(pongClient.Start());
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
        finally
        {
            // Stop listening for new clients.
            _tcpListener.Stop();
        }

        new PongGame(players[0], players[1], messageFactory);

        Debug.Log($"Waiting for game to be done!");
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Debug.Log($"Game is over server will close");
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
