using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    private TcpListener _tcpListener;
    private int _port = 6969;
    private IPAddress _ip = IPAddress.Parse("0.0.0.0");
    private PongGame _game;

    void Start()
    {
        Application.targetFrameRate = 30;
        Application.runInBackground = true;
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

        _game = new PongGame(players[0], players[1], messageFactory);

        Debug.Log($"Waiting for game to be done!");
        //foreach (Thread thread in threads)
        //{
        //    thread.Join();
        //}

        //Debug.Log($"Game is over server will close");
        //Application.Quit();
    }

    private void Update()
    {
        _game?.UpdatePlayersPosition();
    }
}
