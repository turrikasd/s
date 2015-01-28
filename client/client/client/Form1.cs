using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace client
{
    public partial class Form1 : Form
    {
        public Socket WinSocket;
        static TimeSpan tp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Receive();

            //IPEndPoint RemoteEndPoint = new IPEndPoint(
            //IPAddress.Parse("127.0.0.1"), 9050);
            //Socket server = new Socket(AddressFamily.InterNetwork,
            //                           SocketType.Dgram, ProtocolType.Udp);
            //string welcome = "Hello, are you there?";
            //byte[] data = Encoding.ASCII.GetBytes(welcome);
            //server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint); 
        }

        private void Receive()
        {
            IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Any, 9050);
            WinSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            WinSocket.Bind(ServerEndPoint);

            {
                byte[] data = new byte[65507];

                Console.Write("Waiting for client");
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = WinSocket.ReceiveFrom(data, ref Remote);
                Console.WriteLine("Message received from {0}:", Remote.ToString());
                //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

                ByteArrayToFile("moib1.mp3", data);
            }

            for (int i = 0; i < 31; i++)
            {
                //IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Any, 9050);
                //Socket WinSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //WinSocket.Bind(ServerEndPoint);

                byte[] data = new byte[65507];

                Console.Write("Waiting for client");
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = WinSocket.ReceiveFrom(data, ref Remote);
                Console.WriteLine("Message received from {0}:", Remote.ToString());

                AppendArrayToFile("moib1.mp3", data);
            }

            {
                byte[] data = new byte[100];

                Console.Write("Waiting for client");
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = WinSocket.ReceiveFrom(data, ref Remote);
                Console.WriteLine("Message received from {0}:", Remote.ToString());

                mediaPlayer.URL = "moib1.mp3";
            }

            //Thread t = new Thread(SyncMusic);
            //t.Start();

            SyncMusic();
        }

        private void SyncMusic()
        {
            tp = new TimeSpan();

            for (int i = 0; i < 10; i++)
            {
                byte[] data = new byte[100];

                Console.Write("Waiting for client");
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = WinSocket.ReceiveFrom(data, ref Remote);
                Console.WriteLine("Message received from {0}:", Remote.ToString());

                string time = Encoding.ASCII.GetString(data, 0, recv);

                double dTime = double.Parse(time);
                
                if (Math.Abs(mediaPlayer.Ctlcontrols.currentPosition - dTime) > 0.05d)
                    mediaPlayer.Ctlcontrols.currentPosition = double.Parse(time);
            }
        }

        public void AppendArrayToFile(string fileName, byte[] byteArray)
        {
            using (var stream = new FileStream(fileName, FileMode.Append))
            {
                stream.Write(byteArray, 0, byteArray.Length);
            }
        }

        public bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream =
                   new System.IO.FileStream(_FileName, System.IO.FileMode.Create,
                                            System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from
                // a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}",
                                  _Exception.ToString());
            }

            // error occured, return false
            return false;
        }
    }
}
