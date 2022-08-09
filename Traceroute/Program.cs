using System.Net;
using System.Net.Sockets;






class Traceroute
{
    public static void Route(string ip)
    {
        Socket udpHost = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp);
        udpHost.Bind(new IPEndPoint(IPAddress.Parse("192.168.28.110"), 0));
        udpHost.Connect(new IPEndPoint(IPAddress.Parse(ip), 44444));

        Socket ipHost = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        udpHost.Bind(new IPEndPoint(IPAddress.Parse("192.168.28.110"), 0));
        udpHost.Connect(new IPEndPoint(IPAddress.Parse(ip), 0));

        Socket icmpHost = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        udpHost.Bind(new IPEndPoint(IPAddress.Parse("192.168.28.110"), 0));
        udpHost.Connect(new IPEndPoint(IPAddress.Parse(ip), 0));

        
        UDP udp = new UDP();
        udp.setSize(1024);
        udp.calculateChecksum();

        
    }



    class IPPacket
    {
        byte versionAndIhl = 0x45;
        short ipPacketLenth = 0;
        byte ttl = 0;
        byte protocolNumber;
        short checkSum;
        int sourceIp;
        int destinationIp;
        UDP udp;


    }




    class UDP
    {
        short sourcePort;
        short destinationPort;
        short dgramSize;
        short checkSum;
        byte[] dgramData = new byte[1024];


        
        public byte[] getBytes()
        {
            byte[] data = new byte[dgramSize];
            Buffer.BlockCopy(BitConverter.GetBytes(sourcePort), 0, data, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(destinationPort), 0, data, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(dgramSize), 0, data, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(checkSum), 0, data, 6, 2);
            Buffer.BlockCopy(dgramData, 0, data, 8, dgramSize - 8);

            return data;
        }

        public Int16 calculateChecksum()
        {
            checkSum = 0;
            UInt32 chcksm = 0;
            byte[] data = getBytes();
            int index = 0;

            while (index < dgramSize)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);
            checkSum = (Int16)chcksm;
            return (Int16)(~chcksm);
        }

        public void setSize(int size)
        {
            dgramData = new byte[size - 4];
            for (int i = 0; i < size - 4; i++)
                dgramData[i] = (byte)(10);

            dgramSize = (short)(size - 8);
        }
    }
}