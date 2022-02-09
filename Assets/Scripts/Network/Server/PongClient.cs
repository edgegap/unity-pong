using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PongClient
{
    public event Action<PongClient, RawPongMessage> OnMessageReceived;
    public event Action<PongClient> OnConnectionLost;

    private MessageFactory _factory;
    private int _id;
    private TcpClient _client;

    public int ID { get => _id; }

    public PongClient(TcpClient client, int id, MessageFactory factory)
    {
        _factory = factory;
        _id = id;
        _client = client;
        client.NoDelay = true;
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
        NetworkStream stream = _client.GetStream();

        try
        {
            while (true)
            {
                string data = GetNextCommand();
                data = data.Trim();
                Debug.Log($"Receiced ({data}) from player #{_id}");
                OnMessageReceived?.Invoke(this, RawPongMessage.Parse(data));
            }
        }
        catch
        {
            Close();
        }
    }

    public void SendMessage(IPongMessage msg)
    {
        try
        {
            string s = $"{msg.GetMessage()}\n";
            Debug.Log($"Sending ({msg.GetMessage()}) to player #{_id}");
            NetworkStream stream = _client.GetStream();
            var bytesMsg = Encoding.ASCII.GetBytes(s);
            stream.Write(bytesMsg, 0, bytesMsg.Length);
            stream.Flush();
        }
        catch
        {
            Close();
        }
    }

    private void Assign()
    {
        IPongMessage msg = _factory.GetAssignMessage(_id);
        SendMessage(msg);
    }

    private string GetNextCommand()
    {
        StringBuilder sb = new StringBuilder();
        NetworkStream stream = _client.GetStream();
        bool commandCompleted = false;

        while (!commandCompleted)
        {
            int i = stream.ReadByte();

            if (i == -1)
            {
                throw new Exception("End of stream");
            }
            
            char c = (char)i;

            sb.Append(c);

            commandCompleted = c == '\n';
        }

        return sb.ToString();
    }

    private void Close()
    {
        Debug.Log($"Player #{_id} conneciton lost");
        _client.Close();
        OnConnectionLost?.Invoke(this);
    }
}