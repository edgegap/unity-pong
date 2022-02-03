using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerOption
{
    private static string _serverHost;
    private static int _serverPort;
    public string ServerHost { get => _serverHost; set => _serverHost = value; }
    public int ServerPort { get => _serverPort; set => _serverPort = value; }
}
