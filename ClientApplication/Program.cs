using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

class Client
{

    private static readonly string[] Messages = new string[10]
    {
        "Hello from client: {0}. Message 0. \n",
        "Hello from client: {0}. Message 1. \n",
        "Hello from client: {0}. Message 2. \n",
        "Hello from client: {0}. Message 3. \n",
        "Hello from client: {0}. Message 4. \n",
        "Hello from client: {0}. Message 5. \n",
        "Hello from client: {0}. Message 6. \n",
        "Hello from client: {0}. Message 7. \n",
        "Hello from client: {0}. Message 8. \n",
        "Hello from client: {0}. Message 9. \n"
    };

    static void Main(string[] args)
    {
        Thread t1 = new Thread(() => ConnectToServer("Client 1", "127.0.0.1", 33333));
        Thread t2 = new Thread(() => ConnectToServer("Client 2", "127.0.0.1", 33333));
        Thread t3 = new Thread(() => ConnectToServer("Client 3", "127.0.0.1", 33333));

        t1.Start();
        t2.Start();
        t3.Start();
    }

    static void ConnectToServer(string clientName, string ip, int port)
    {
        IPAddress ipAddress = IPAddress.Parse(ip);

        TcpClient tcpClientSocket = new TcpClient();

        tcpClientSocket.Connect(ipAddress, port);

        Console.WriteLine(String.Format("Client {0} got connected to server.", clientName));

        NetworkStream networkStream = tcpClientSocket.GetStream();

        SendClientsName(clientName, networkStream);
        ReceiveInitialMessages(networkStream);
        SendMessagesToServer(networkStream, clientName);
        ReceiveBroadcastedMessages(networkStream);
        WaitForServerToDisconnect(tcpClientSocket, networkStream);
        networkStream.Close();
        tcpClientSocket.Close();
    }

    private static void SendClientsName(string clientName, NetworkStream networkStream)
    {
        byte[] clientsNameToSend = Encoding.ASCII.GetBytes(clientName + "\n");
        networkStream.Write(clientsNameToSend, 0, clientsNameToSend.Length);
    }

    private static void ReceiveInitialMessages(NetworkStream networkStream)
    {
        StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);

        while (true)
        {
            string messageFromServer = reader.ReadLine();
            if (messageFromServer != null && messageFromServer != "Finished")
            {
                Console.WriteLine(messageFromServer);
            }
            else
            {
                Console.WriteLine("Server sent initial messages.");
                break;
            }
        }
    }

    private static void SendMessagesToServer(NetworkStream networkStream, string clientName)
    {
        var random = new Random();
        int messageCount = random.Next(1, 3);

        for (int i = 0; i <= messageCount + 1; i++)
        {
            if (i == messageCount + 1)
            {
                byte[] finishedBytesToServer = Encoding.ASCII.GetBytes("Finished\n");
                networkStream.Write(finishedBytesToServer, 0, finishedBytesToServer.Length);
            }
            else
            {
                int messageIndexToSend = random.Next(0, Client.Messages.Length - 1);
                string messageToSend = Client.Messages[messageIndexToSend];

                byte[] bytesToServer = Encoding.ASCII.GetBytes(String.Format(messageToSend, clientName));
                networkStream.Write(bytesToServer, 0, bytesToServer.Length);
            }

            var delay = random.Next(3000, 4000);
            Thread.Sleep(delay);
        }
    }

    private static void ReceiveBroadcastedMessages(NetworkStream networkStream)
    {
        StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
        Console.WriteLine("I'm listening to server");

        while (true)
        {
            string messageFromServer = reader.ReadLine();
            if (messageFromServer != null && messageFromServer != "Finished")
            {
                Console.WriteLine(messageFromServer);
            }
            else
            {
                Console.WriteLine("Server sent messages from other clients.");
                break;
            }
        }
    }

    private static void WaitForServerToDisconnect(TcpClient tcpClientSocket, NetworkStream networkStream)
    {
        StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
        Console.WriteLine("I'm listening to server to disconnect.");

        while (true)
        {
            string messageFromServer = reader.ReadLine();
            if (messageFromServer != null && messageFromServer == "close")
            {
                Console.WriteLine("Disconnecting from server");
                tcpClientSocket.Close();
                break;
            }
        }
    }
}