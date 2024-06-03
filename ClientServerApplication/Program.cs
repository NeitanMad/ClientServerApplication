using NetMQ.Sockets;
using NetMQ;

public interface IMessageSourceClient
{
    event EventHandler<string> MessageReceived;
    void Connect(string address);
    void SendMessage(string message);
    void Disconnect();
}

public class NetMQClient : IMessageSourceClient
{
    private readonly string address;
    private RequestSocket clientSocket;
    private NetMQPoller poller;
    private Thread clientThread;

    public event EventHandler<string> MessageReceived;

    public static void Main(string[] args)
    {
        NetMQClient client = new NetMQClient("tcp://localhost:5556");
        client.Connect("tcp://localhost:5556");

        Console.WriteLine("Подключение к серверу..");
        Console.WriteLine("Для выхода введите 'exit'");

        while (true)
        {
            Console.Write("Введите сообщение: ");
            string message = Console.ReadLine();

            if (message.ToLower() == "exit")
                break;

            client.SendMessage(message);
        }

        client.Disconnect();
    }

    public NetMQClient(string address)
    {
        this.address = address;
    }

    public void Connect(string address)
    {
        clientSocket = new RequestSocket();
        clientSocket.Connect(address);

        poller = new NetMQPoller { clientSocket };
        clientSocket.ReceiveReady += OnReceiveReady;

        clientThread = new Thread(poller.Run) { IsBackground = true };
        clientThread.Start();
    }

    private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        var message = e.Socket.ReceiveFrameString();
        MessageReceived?.Invoke(this, message);
    }

    public void SendMessage(string message)
    {
        clientSocket.SendFrame(message);
    }

    public void Disconnect()
    {
        poller.Stop();
        clientSocket.Close();
        clientSocket.Dispose();
        poller.Dispose();
    }
}
