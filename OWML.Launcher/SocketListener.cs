﻿using OWML.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OWML.Launcher
{
    public class SocketListener
    {
        private static int _port;

        public SocketListener(int port)
        {
            _port = port;
            new Task(SetupListener).Start();
        }

        private void SetupListener()
        {
            TcpListener server = null;
            try
            {
                var localAddress = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddress, _port);
                server.Start();

                var bytes = new byte[1024];
                string data = null;

                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");

                    var client = server.AcceptTcpClient();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Console connected to socket!");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    data = null;

                    var stream = client.GetStream();

                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        var objects = data.Split(new string[] { ";;" }, StringSplitOptions.None);

                        if (objects[1] == Constants.QuitKeyPhrase)
                        {
                            Environment.Exit(0);
                        }
                        
                        Console.WriteLine("[" + objects[0] + "] : " + objects[1]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in socket listener : " + ex);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            finally
            {
                server.Stop();
            }
        }
    }
}
