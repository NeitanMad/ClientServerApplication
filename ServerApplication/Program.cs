using NetMQ.Sockets;
using NetMQ;

public interface IMessageSource
{
    event EventHandler<string> MessageReceived;
    void SendMessage(string message);
}

public class NetMQServer : IMessageSource
{
    private readonly string address;
    private NetMQSocket serverSocket;
    private NetMQPoller poller;
    private Thread serverThread;

    public event EventHandler<string> MessageReceived;

    public static void Main(string[] args)
    {
        NetMQServer server = new NetMQServer("tcp://*:5556");
        server.Start();

        Console.WriteLine("Сервер запущен!");
        Console.ReadLine();

        server.Stop();
    }

    public NetMQServer(string address)
    {
        this.address = address;
    }

    public void Start()
    {
        serverSocket = new ResponseSocket();
        serverSocket.Bind(address);

        poller = new NetMQPoller { serverSocket };
        serverSocket.ReceiveReady += OnReceiveReady;

        serverThread = new Thread(poller.Run) { IsBackground = true };
        serverThread.Start();
    }

    private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        var message = e.Socket.ReceiveFrameString();
        MessageReceived?.Invoke(this, message);
        e.Socket.SendFrame("Ack");
    }

    public void SendMessage(string message)
    {
        serverSocket?.SendFrame(message);
    }

    public void Stop()
    {
        poller?.Stop();
        serverSocket?.Close();
        serverSocket?.Dispose();
        poller?.Dispose();
    }
}
