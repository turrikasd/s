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
        static Socket senderSock;
        static IPEndPoint remoteEP;
        static int progress = 0;
        static int update = -1;

        string curFileName = "music.mp3";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DownloadFileSyncAndPlay();
        }

        private void DownloadFileSyncAndPlay()
        {
        // Data buffer for incoming data.
        byte[] bytes = new byte[1024];

        // Connect to a remote device.
        try {
            // Establish the remote endpoint for the socket.
            //// This example uses port 11000 on the local computer.
            IPHostEntry ipHostInfo = Dns.Resolve("169.254.160.127");
            //IPHostEntry ipHostInfo = Dns.Resolve("192.168.43.16");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            remoteEP = new IPEndPoint(ipAddress,11000);

            // Create a TCP/IP  socket.
            senderSock = new Socket(AddressFamily.InterNetwork, 
                SocketType.Stream, ProtocolType.Tcp );

            // Connect the socket to the remote endpoint. Catch any errors.
            try {
                senderSock.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    senderSock.RemoteEndPoint.ToString());

                // Encode the data string into a byte array.
                byte[] msg = Encoding.ASCII.GetBytes("Hey");

                // Send the data through the socket.
                int bytesSent = senderSock.Send(msg);

                // Receive file!
                Console.WriteLine("Starting to receive a file...");
                senderSock.ReceiveTimeout = 150;
                curFileName = "music" + update.ToString() + ".mp3";

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        FileStream fs = File.OpenWrite(curFileName);
                        fs.Close();
                        break;
                    }

                    catch
                    {
                        curFileName = "music" + i + update.ToString() + ".mp3";
                    }
                }

                using (var output = File.Create(curFileName))
                {
                    // read the file in chunks of 1KB
                    var buffer = new byte[1024];
                    int bytesRead;
                    try
                    {
                        while ((bytesRead = senderSock.Receive(buffer, 1024, SocketFlags.None)) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                        }
                    }
                    catch
                    { }
                }

                Console.WriteLine("Done receiving file!");

                senderSock.Send(Encoding.ASCII.GetBytes("done"));

                // Receive the response from the remote device.
                int bytesRec = senderSock.Receive(bytes);
                update = int.Parse(Encoding.ASCII.GetString(bytes,0,bytesRec));

                // Send Pingtest back
                msg = Encoding.ASCII.GetBytes("ping");
                bytesSent = senderSock.Send(msg);

                // Receive the play order from the remote device.
                bytesRec = senderSock.Receive(bytes);
                long sleepTime = long.Parse(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                progress = 0;

                label2.Text = (sleepTime / 1000).ToString();


                if (sleepTime <= 0)
                {
                    progress = Math.Abs((int)sleepTime) + 4000;
                    sleepTime = 4000;
                }

                else if (sleepTime < 4000)
                {
                    progress = 4000 - (int)sleepTime;
                    sleepTime = 4000;
                }

                timer1.Interval = (int)sleepTime;
                timer1.Start();

                mediaPlayer.URL = curFileName;

                senderSock.Shutdown(SocketShutdown.Both);
                senderSock.Close();

                if (!timer2.Enabled)
                {
                    timer2.Interval = 10000;
                    timer2.Start();
                }
                
            } catch (ArgumentNullException ane) {
                Console.WriteLine("ArgumentNullException : {0}",ane.ToString());
            } catch (SocketException se) {
                Console.WriteLine("SocketException : {0}",se.ToString());
            } catch (Exception e) {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        } catch (Exception e) {
            Console.WriteLine( e.ToString());
        }
        }

        static void SendFile(string fileName)
        {
            // Create a TCP/IP  socket.
            senderSock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            senderSock.Connect(remoteEP);
            byte[] msg = Encoding.ASCII.GetBytes("file");
            int bytesSent = senderSock.Send(msg);
            senderSock.SendFile(fileName);

            senderSock.Shutdown(SocketShutdown.Both);
            senderSock.Close();
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progress == 0)
                mediaPlayer.Ctlcontrols.currentPosition = 0.0d;
            else
                mediaPlayer.Ctlcontrols.currentPosition = progress / 1000.0d;

            timer1.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            SendFile(openFileDialog1.FileName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Release the socket.
            //senderSock.Shutdown(SocketShutdown.Both);
            //senderSock.Close();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // Create a TCP/IP  socket.
            senderSock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            senderSock.Connect(remoteEP);
            byte[] msg = Encoding.ASCII.GetBytes("update");
            senderSock.Send(msg);

            byte[] bytes = new byte[1024];
            int bytesRec = senderSock.Receive(bytes);
            int recUp = int.Parse(Encoding.ASCII.GetString(bytes, 0, bytesRec));

            if (recUp > update)
            {
                Console.WriteLine("Need update!");

                DownloadFileSyncAndPlay();
            }

            else
            {
                // Release the socket.
                senderSock.Shutdown(SocketShutdown.Both);
                senderSock.Close();
            }
        }

        private void songTime_Tick(object sender, EventArgs e)
        {
            label1.Text = ((int)(mediaPlayer.Ctlcontrols.currentPosition)).ToString();
        }
    }
}
