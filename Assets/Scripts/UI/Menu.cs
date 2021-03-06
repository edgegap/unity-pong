using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    InputField host;

    [SerializeField]
    Button ConnectButton;
    [SerializeField]
    Button FindMatchButton;
    [SerializeField]
    Button CancelButton;

    [SerializeField]
    Text WaitText;

    // If you want to change the matchmaker just change the factory here
    private static IMatchmakerFactory _factory = new ArbitriumMatchmakerFactory(); 
    private IMatchmaker _matchmaker = _factory.CreateMatchmaker();
    private PendingTicket _pendingTicket;

    void Start()
    {
        // Set game scree resolution
        Screen.SetResolution(1000, 700, false);

        // Set handlers
        ConnectButton.GetComponentInChildren<Text>().text = "Connect";
        ConnectButton.onClick.AddListener(HandleClickConnect);
        FindMatchButton.onClick.AddListener(HandleFindMatch);
        CancelButton.onClick.AddListener(HandleCancel);
        CancelButton.enabled = false;
        FindMatchButton.enabled = true;

        Application.runInBackground = true;
    }

    void HandleClickConnect()
    {
        string[] splitHost = host.text.Split(':');
        Connect(splitHost[0], Convert.ToInt32(splitHost[1]));
    }

    async void HandleFindMatch()
    {
        CancelButton.enabled = true;
        FindMatchButton.enabled = false;
        WaitText.text = "Waiting for player";
        _pendingTicket = await _matchmaker.CreateTicket(Mode.casual);
        Debug.Log($"Finding match {_pendingTicket.Id}");

        try
        {
            ReadyTicket ticket = await _matchmaker.ResolveTicket(_pendingTicket, 30);
            Connect(ticket.Ip, ticket.Port);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            await _matchmaker.DeleteTicket(_pendingTicket);
            WaitText.text = "";
        }
    }

    async void HandleCancel()
    {
        CancelButton.enabled = false;
        FindMatchButton.enabled = true;
        Debug.Log("Cancel");
        await _matchmaker.DeleteTicket(_pendingTicket);
        WaitText.text = "";
    }

    private void Connect(string ip, int port)
    {
        ServerOption option = new ServerOption();
        option.ServerHost = ip;
        option.ServerPort = port;

        SceneManager.LoadScene("Arena", LoadSceneMode.Single);
    }
}
