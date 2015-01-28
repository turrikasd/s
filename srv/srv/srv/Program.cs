using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace srv
{
    class Program
    {
        static void Main(string[] args)
        {
            Send();
        }

        static void Send()
        {
            IPEndPoint RemoteEndPoint = new IPEndPoint(
                                       IPAddress.Parse("192.168.43.231"), 9050);

            IPEndPoint RemoteEndPoint2 = new IPEndPoint(
                                       IPAddress.Parse("192.168.43.16"), 9050);

            Socket server = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Dgram, ProtocolType.Udp);

            byte[] music;
            music = File.ReadAllBytes("moib1.mp3");

            for (int i = 0; i < 32; i++)
            {
                byte[] data = new byte[65507];

                Array.Copy(music, i * 65507, data, 0, 65507);

                server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint);
                server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint2);

                Console.WriteLine((int)(i / 32.0f * 100.0f) + "% done");
                System.Threading.Thread.Sleep(100);
            }

            {
                Console.ReadKey();
                byte[] data = Encoding.ASCII.GetBytes("play");
                server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint);
                server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint2);
            }

            Stopwatch s = new Stopwatch();
            s.Start();

            for (int i = 0; i < 10; i++)
            {
                double dtime = (double)(s.ElapsedMilliseconds / 1000.0d);
                string time = dtime.ToString();
                byte[] data = Encoding.ASCII.GetBytes(time);
                server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint);
                server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint2);

                System.Threading.Thread.Sleep(1000);
            }
        }

        static void Recieve()
        {
            IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Any, 9050);
            Socket WinSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            WinSocket.Bind(ServerEndPoint);

            byte[] data = new byte[65507];

            Console.Write("Waiting for client");
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            int recv = WinSocket.ReceiveFrom(data, ref Remote);
            Console.WriteLine("Message received from {0}:", Remote.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

            Console.ReadKey();
        }
    }
}
