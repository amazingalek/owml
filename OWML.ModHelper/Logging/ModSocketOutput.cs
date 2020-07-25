﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using OWML.Common;

namespace OWML.ModHelper
{
    public class ModSocketOutput : ModConsole
    {
        private readonly int _port;
        private static Socket _socket;
        private static IModLogger _logger;

        public ModSocketOutput(IOwmlConfig config, IModLogger logger, IModManifest manifest) : base(config, logger, manifest)
        {
            if (_socket == null)
            {
                _port = config.SocketPort;
                ConnectToSocket();
                _logger = logger;
            }
        }

        [Obsolete("Use WriteLine(string) or WriteLine(string, MessageType) instead.")]
        public override void WriteLine(params object[] objects)
        {
            var line = string.Join(" ", objects.Select(o => o.ToString()).ToArray());
            var type = MessageType.Message;
            WriteLine(type, line, GetCallingMethodName(new StackTrace()));
        }

        public override void WriteLine(string line)
        {
            var type = MessageType.Message;
            WriteLine(type, line, GetCallingMethodName(new StackTrace()));
        }

        public override void WriteLine(string line, MessageType type)
        {
            WriteLine(type, line, GetCallingMethodName(new StackTrace()));
        }

        private void WriteLine(MessageType type, string line, string senderType)
        {
            Logger?.Log(line);
            CallWriteCallback(Manifest, line);

            var message = new SocketMessage
            {
                SenderName = Manifest.Name,
                SenderType = senderType,
                Type = type,
                Message = line
            };
            var json = JsonConvert.SerializeObject(message);

            WriteToSocket(json);
        }

        private string GetCallingMethodName(StackTrace frame)
        {
            try
            {
                return frame.GetFrame(1).GetMethod().DeclaringType.Name;
            }
            catch
            {
                return "";
            }
        }

        private void ConnectToSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipAddress = IPAddress.Parse(Constants.LocalAddress);
            var endPoint = new IPEndPoint(ipAddress, _port);
            _socket.Connect(endPoint);
        }

        private void WriteToSocket(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            _socket?.Send(bytes);
        }
    }
}
