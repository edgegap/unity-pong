using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConfiguration
{ 
    public string ListenIp { get; private set; }
    public int Port { get; private set; }
    public int TargetFrameRate { get; private set; }
    private ServerConfiguration(string ip, int port, int frame)
    {
        ListenIp = ip;
        Port = port;
        TargetFrameRate = frame;
    }

    public static ServerConfiguration GetFromEnvironmentVariables()
    {
        string portValue = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "20105";
        string listenIp = Environment.GetEnvironmentVariable("LISTEN_IP") ?? "0.0.0.0";
        string targetFrameRate = Environment.GetEnvironmentVariable("TARGET_FRAME_RATE") ?? "30";
        return new ServerConfiguration(listenIp, Convert.ToInt32(portValue), Convert.ToInt32(targetFrameRate));
    }
}
