using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class PongGameFactory
{
    public PongGame CreateGame(ServerConfiguration configuration)
    {
        MessageFactory messageFactory = new MessageFactory();

        PongGame game = new PongGame(messageFactory, new RawMessageInterpreter());


        Thread mainThread = Thread.CurrentThread;

        Action handleGameOver = () => mainThread.Abort();

        // Stop server if player disconnect before game was created
        game.OnGameOver += handleGameOver;
        TcpListener tcpListener = null;

        try
        {
            int playerCount = 0;
            tcpListener = new TcpListener(IPAddress.Parse(configuration.ListenIp), configuration.Port);
            tcpListener.Start();

            while (playerCount != 2)
            {
                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                int playerId = playerCount + 1;
                TcpClient client = tcpListener.AcceptTcpClient();
                Debug.Log($"Player #{playerId} connected!");

                PongClient pongClient = new PongClient(client, playerId, messageFactory);
                game.AddPlayer(pongClient);
                pongClient.Start();

                playerCount++;
            }
        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            // Stop listening for new clients.
            tcpListener?.Stop();
        }

        game.OnGameOver -= handleGameOver;

        return game;
    }
}
