using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerCommunicator: MonoBehaviour
{
    public event Action<string> OnMessageReceived;
    private System.IO.StreamWriter _writter;
    private System.IO.StreamReader _reader;
    private NetworkStream _stream;
    private TcpClient _client;

    void Start()
    {
        ConnectToServer();
        InitStream();
    }

    public void SendMessageServer(string msg)
    {
        try
        {
            _writter.WriteLine(msg);
            _writter.Flush();
        } catch (Exception e) // TODO Change for a better Exception then Exception
        {
            Debug.LogError(e);
            ReturnToMenu();
        }
    }

    void Update()
    {
        if (_stream?.DataAvailable ?? false)
        {
            try
            {
                OnMessageReceived?.Invoke(_reader.ReadLine());
            }
            catch (Exception e) // TODO Change for a better Exception then Exception
            {
                Debug.LogError(e);
                ReturnToMenu();
            }
        }
    }

    private void ConnectToServer()
    {
        try
        {
            ServerOption option = new ServerOption();
            _client = new TcpClient();
            _client.Connect(option.ServerHost, option.ServerPort);
        }
        catch (Exception e) // TODO Change for a better Exception then Exception
        {
            Debug.LogError(e);
            ReturnToMenu();
        }
    }
    private void InitStream()
    {
        try
        {
            _stream = _client.GetStream();
            _writter = new System.IO.StreamWriter(_client.GetStream());
            _reader = new System.IO.StreamReader(_client.GetStream());
        }
        catch (Exception e) // TODO Change for a better Exception then Exception
        {
            Debug.LogError(e);
            ReturnToMenu();
        }
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
