using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

class Program
{
    public static void Main(string[] args)
    {
        string remotePoint;
        int sourceIp;
        int packetSize;
        int maxTtl;
        int attempts;
        int timeout;

        try
        {



            if (args[0] == "icmp")
            {
                remotePoint = args[1];
                packetSize = int.Parse(args[2]);
                maxTtl = int.Parse(args[3]);
                attempts = int.Parse(args[4]);
                timeout = int.Parse(args[5]);
                Traceroute.IcmpTraceroute(remotePoint, packetSize, maxTtl, attempts, timeout);
            }
            else if (args[0] == "udp")
            {
                remotePoint = args[1];
                sourceIp = int.Parse(args[2]);
                packetSize = int.Parse(args[3]);
                maxTtl = int.Parse(args[4]);
                attempts = int.Parse(args[5]);
                timeout = int.Parse(args[6]);
                Traceroute.UdpTraceroute(remotePoint, sourceIp, packetSize, maxTtl, attempts, timeout);
            }
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OverflowException || ex is FormatException || ex is IndexOutOfRangeException)
        {
            Console.WriteLine("Invalid command line arguments\n" +
                                "\tTraceroute [PROTOCOL (icmp | udp)] [REMOTE IP:REMOTE PORT (127.0.0.1:1234)] [SOURCE PORT (udp only)] [SIZE] [MAX TTL] [ATTEMPTS] [TIMEOUT]");
            return;
        }

    }
}

class Traceroute
{
    static byte[] buffer = new byte[65536];
    
    public static void UdpTraceroute(string remotePoint, int sourcePort, int packetSize, int maxTtl, int attempts, int timeout)
    {
        string remoteIp = remotePoint.Split(":")[0];
        int remotePort = int.Parse(remotePoint.Split(":")[1]);

        IPEndPoint remoteIPEndPoint = null;
        UdpClient udp = new UdpClient(sourcePort);

        try { remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort); }
        catch (FormatException) { }

        udp.Connect(remoteIPEndPoint);
        IPEndPoint localIPEndPoint = (IPEndPoint)udp.Client.LocalEndPoint;
        udp.Close();

        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp);
        udpSocket.Bind(localIPEndPoint);
        udpSocket.Connect(remoteIPEndPoint);

        Socket icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        icmpSocket.Bind(localIPEndPoint);
        icmpSocket.ReceiveTimeout = timeout;

        byte[] packet = new byte[packetSize];
        Buffer.BlockCopy(BitConverter.GetBytes(localIPEndPoint.Port), 0, packet, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(remoteIPEndPoint.Port), 0, packet, 2, 2);
        packet[5] = (byte)packet.Length;
        packet[6] = 0;
        packet[7] = 0;

        string nodeIp = "";
        string hostName = "";
        int timeoutCounter;

        try { hostName = Dns.GetHostEntry(remoteIp).HostName; }
        catch (Exception) { }
         
        Console.WriteLine($"Traceroute to {hostName} {remoteIp}\n" +
            $"with maximum number of hops {maxTtl}");
        

        for (udpSocket.Ttl = 1; udpSocket.Ttl < maxTtl && nodeIp != remoteIp; udpSocket.Ttl++)
        {
            int attemptsFailed = 0;
            Console.Write($"{udpSocket.Ttl}\t");
            for (int i = 0; i < attempts; i++)
            {

                udpSocket.Send(packet, packet.Length, SocketFlags.None);
                buffer = new byte[65536];

                try
                {
                    DateTime sentTime = DateTime.Now;
                    icmpSocket.Receive(buffer, SocketFlags.None);
                    DateTime recievedTime = DateTime.Now;
                    Console.Write($"{(int)(recievedTime-sentTime).TotalMilliseconds}ms \t");
                    timeoutCounter = 0;
                }
                catch (SocketException)
                {
                    Console.Write("*\t");
                    attemptsFailed++;
                }
            }

            if (attemptsFailed == attempts)
            {
                Console.WriteLine("Timed out request");
            }
            else
            {
                nodeIp = string.Join(".", buffer.Skip(12).Take(4));
                try { hostName = Dns.GetHostEntry(nodeIp).HostName; }
                catch (Exception) { }

                Console.WriteLine($"{hostName} [{nodeIp}]");
            }

        }
    }

    public static void IcmpTraceroute(string destinationIp, int packetSize, int maxTtl, int attempts, int timeout)
    {
        IPEndPoint remoteIPEndPoint = null;
        try { remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(destinationIp), 55555); }
        catch (FormatException) { }

        Socket icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        icmpSocket.ReceiveTimeout = timeout;
        icmpSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.28.110"), 0));

        string nodeIp = "";
        string hostName = "";
        int timeoutCounter;

        try { hostName = Dns.GetHostEntry(destinationIp).HostName; }
        catch (Exception) { }

        Console.WriteLine($"Traceroute to {hostName} {destinationIp}\n" +
            $"with maximum number of hops 40");


        for (icmpSocket.Ttl = 1; icmpSocket.Ttl < maxTtl && nodeIp != destinationIp; icmpSocket.Ttl++)
        {
            int attemptsFailed = 0;
            Console.Write($"{icmpSocket.Ttl}\t");
            for (int i = 0; i < attempts; i++)
            {
                Icmp icmp = new Icmp();
                icmp.setSize(packetSize);
                icmp.checkSum = icmp.getChecksum();
                icmpSocket.SendTo(icmp.getBytes(), SocketFlags.None, remoteIPEndPoint);

                buffer = new byte[65536];

                try
                {
                    DateTime sentTime = DateTime.Now;
                    icmpSocket.Receive(buffer, SocketFlags.None);
                    DateTime recievedTime = DateTime.Now;
                    Console.Write($"{(int)(recievedTime - sentTime).TotalMilliseconds}ms \t");
                    timeoutCounter = 0;
                }
                catch (SocketException)
                {
                    Console.Write("*\t");
                    attemptsFailed++;
                }
            }

            if (attemptsFailed == attempts)
            {
                Console.WriteLine("Timed out request");
            }
            else
            {
                nodeIp = string.Join(".", buffer.Skip(12).Take(4));
                try { hostName = Dns.GetHostEntry(nodeIp).HostName; }
                catch (Exception) { }

                Console.WriteLine($"{hostName} [{nodeIp}]");
            }

        }


    }

    struct Icmp
    {
        public byte type;
        public byte code;
        public Int16 checkSum;
        private int messageSize;
        public int MessageSize
        {
            get { return messageSize; }
            set { messageSize = value; message = new byte[value]; }
        }
        public byte[] message;

        public Icmp()
        {
            type = 0x08;
            code = 0x00;
            checkSum = 0x00;
            messageSize = 0;
            message = new byte[0];
        }

        public Icmp(string data)
        {
            type = 0x08;
            code = 0x00;
            checkSum = 0x00;
            message = Encoding.UTF8.GetBytes(data);
            messageSize = Encoding.UTF8.GetBytes(data).Length;
        }

        public Icmp(byte[] data, int size)
        {
            type = data[20];
            code = data[21];
            checkSum = BitConverter.ToInt16(data, 22);
            messageSize = size - 24;
            message = new byte[messageSize];
            Buffer.BlockCopy(data, 24, message, 0, messageSize);
        }

        public byte[] getBytes()
        {
            byte[] data = new byte[messageSize + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checkSum), 0, data, 2, 2);
            Buffer.BlockCopy(message, 0, data, 4, messageSize);
            return data;
        }

        public Int16 getChecksum()
        {
            checkSum = 0;
            UInt32 chcksm = 0;
            byte[] data = getBytes();
            int packetsize = MessageSize + 4;
            int index = 0;

            while (index < packetsize)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);
            return (Int16)(~chcksm);
        }
        public void setSize(int size)
        {
            message = new byte[size - 4];
            for (int i = 0; i < size - 4; i++)
                message[i] = (byte)(10);

            messageSize = size - 4;
        }
    }
}