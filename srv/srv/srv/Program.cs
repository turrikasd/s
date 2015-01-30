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
        // Incoming data from the client.
        public static string data = null;
        static int update = 0;
        static string curFileName = "song.mp3";

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve("0.0.0.0");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Stopwatch startTimer = new Stopwatch();
                startTimer.Start();

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        break;
                    }

                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", data);

                    if (data == "Hey")
                    {
                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        handler.SendFile(curFileName);

                        byte[] rec = new byte[1024];
                        int bytesRe = handler.Receive(rec);

                        Console.WriteLine("Rec: " + Encoding.ASCII.GetString(rec, 0, bytesRe));

                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        msg = Encoding.ASCII.GetBytes(update.ToString());
                        handler.Send(msg);

                        bytes = new byte[1024];
                        bytesRe = handler.Receive(bytes);

                        Console.WriteLine("Rec: " + Encoding.ASCII.GetString(bytes, 0, bytesRe));

                        Console.WriteLine("Ping: " + sw.ElapsedMilliseconds);

                        string sendTimer = "" + (sw.ElapsedMilliseconds / 2 + 10000 - startTimer.ElapsedMilliseconds);
                        msg = Encoding.ASCII.GetBytes(sendTimer);

                        sw.Stop();

                        handler.Send(msg); // send play order

                        Console.WriteLine(sendTimer);
                    }

                    else if (data == "file")
                    {
                        startTimer.Reset();
                        startTimer.Start();

                        Console.WriteLine("Starting to receive a file...");
                        handler.ReceiveTimeout = 1000;

                        using (var output = File.Create("song.mp3"))
                        {
                            // read the file in chunks of 1KB
                            var buffer = new byte[1024];
                            int bytesRead;
                            try
                            {
                                while ((bytesRead = handler.Receive(buffer, 1024, SocketFlags.None)) > 0)
                                {
                                    output.Write(buffer, 0, bytesRead);
                                }
                            }
                            catch
                            { }
                        }

                        update++;
                        Console.WriteLine("Done receiving file!");
                    }

                    else if (data == "update")
                    {
                        bytes = new byte[1024];
                        bytes = Encoding.ASCII.GetBytes("" + update);
                        handler.Send(bytes);
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}
