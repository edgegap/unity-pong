using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PongClient
{
    public event Action<PongClient, RawPongMessage> OnMessageReceived;

    private MessageFactory _factory;
    private int _id;
    private TcpClient _client;
    private StreamWriter _writer;

    public int ID { get => _id; }

    public PongClient(TcpClient client, int id, MessageFactory factory)
    {
        _factory = factory;
        _id = id;
        _client = client;
        client.NoDelay = true;
        _writer = new StreamWriter(_client.GetStream());
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
        string s = $"{msg.GetMessage()}\n";
        Debug.Log($"Sending ({msg.GetMessage()}) to player #{_id}");
        //_writer.WriteLine(s);
        //_writer.Flush();
        //_client.GetStream().Flush();
        NetworkStream stream = _client.GetStream();
        var bytesMsg = Encoding.ASCII.GetBytes(s);
        stream.Write(bytesMsg, 0, bytesMsg.Length);
        stream.Flush();
    }

    private void Assign()
    {
        IPongMessage msg = _factory.GetAssignMessage(_id);
        _writer.WriteLine(msg.GetMessage());
        _writer.Flush();
    }

    private string GetNextCommand()
    {
        StringBuilder sb = new StringBuilder();
        NetworkStream stream = _client.GetStream();
        bool commandCompleted = false;
        while (!commandCompleted)
        {
            char c = (char)stream.ReadByte();
            sb.Append(c);

            commandCompleted = c == '\n';
        }
        
        return sb.ToString();
    }
}