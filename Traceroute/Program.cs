using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

class Program
{
    public static void Main(string[] args)
    {
        string destinationIp;
        int packetSize;
        int maxTtl;
        int attempts;
        int timeout;

        try
        {
            //destinationIp = args[0];
            //packetSize = int.Parse(args[1]);
            //maxTtl = int.Parse(args[2]);
            //attempts = int.Parse(args[3]);
            //timeout = int.Parse(args[4]);

            destinationIp = "8.8.8.8";
            packetSize = 1024;
            maxTtl = 50;
            attempts = 3;
            timeout = 250;
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OverflowException || ex is FormatException || ex is IndexOutOfRangeException)
        {
            Console.WriteLine("Invalid command line arguments\n" +
                                "\tMyPing [IP] [SIZE] [MAX TTL] [ATTEMPTS] [TIMEOUT]");
            return;
        }

        Traceroute.Route(destinationIp, packetSize, maxTtl, attempts, timeout);
    }
}

class Traceroute
{
    static string sourceIp = "192.168.28.110";
    static byte[] buffer = new byte[65536];
    
    public static void Route(string destinationIp, int packetSize, int maxTtl, int attempts, int timeout)
    {
        Socket icmpHost = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        icmpHost.Bind(new IPEndPoint(IPAddress.Parse(sourceIp), 0));
        icmpHost.ReceiveTimeout = timeout;

        Socket udpHost = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpHost.Bind(new IPEndPoint(IPAddress.Parse(sourceIp), 0));

        Icmp icmp = new Icmp();
        icmp.setSize(packetSize);
        EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
        EndPoint ra = new IPEndPoint(IPAddress.Parse(destinationIp), 0);

        string nodeIp = "";
        string hostName = "";
        int timeoutCounter = 0;

        try { hostName = Dns.GetHostEntry(nodeIp).HostName; }
        catch (Exception) { }

        Console.WriteLine($"Traceroute to {hostName} {destinationIp}\n" +
            $"with maximum number of hops 40");
        

        for (short i = 0; i < maxTtl && nodeIp != destinationIp; i++)
        {
            icmpHost.Ttl = i;
            icmp.checkSum = icmp.getChecksum();
            Console.Write($"{i + 1}\t");
            for (int k = 0; k < attempts; k++)
            {
                icmpHost.SendTo(icmp.getBytes(), ra);
                //udpHost.SendTo(buffer.Take(packetSize).ToArray(), ra);
                try
                {
                    DateTime sentTime = DateTime.Now;
                    icmpHost.ReceiveFrom(buffer, ref remoteIp);
                    DateTime recievedTime = DateTime.Now;
                    Console.Write($"{(int)(recievedTime-sentTime).TotalMilliseconds}ms \t");
                    timeoutCounter = 0;
                }
                catch (SocketException)
                {
                    Console.Write("*\t");
                    timeoutCounter++;
                    if (timeoutCounter > 30) 
                    {
                        Console.WriteLine("Remote host not responding");
                        return;
                    }
                }
            }
            nodeIp = string.Join(".", buffer.Skip(12).Take(4));
            try { hostName = Dns.GetHostEntry(nodeIp).HostName; }
            catch (Exception) { }

            Console.WriteLine($"{hostName} [{nodeIp}]");
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