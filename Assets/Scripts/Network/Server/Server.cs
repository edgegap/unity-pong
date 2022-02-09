using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    private PongGame _game;

    void Start()
    {
        ServerConfiguration config = ServerConfiguration.GetFromEnvironmentVariables();

        Debug.Log("Configuration loaded");
        Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));

        Application.targetFrameRate = config.TargetFrameRate;
        Application.runInBackground = true;

        try
        {
            _game = new PongGameFactory().CreateGame(config);
            _game.OnGameOver += Quit;

            Debug.Log("Starting Game");
            _game.Start();
        }
        catch
        {
            Quit();
        }
    }

    private void Update()
    {
        _game?.UpdatePlayersPosition();
    }

    private void Quit()
    {
        Application.Quit();
    }
}
