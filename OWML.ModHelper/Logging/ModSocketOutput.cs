﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using OWML.Common;

namespace OWML.ModHelper.Logging
{
    public class ModSocketOutput : ModConsole
    {
        private int _port;
        private static Socket _socket;

        public ModSocketOutput(IOwmlConfig config, IModLogger logger, IModManifest manifest) : base(config, logger, manifest)
        {
            if (_socket == null)
            {
                _port = config.SocketPort;
                ConnectToSocket();
            }
        }

        [Obsolete]
        public override void WriteLine(params object[] objects)
        {
            var line = string.Join(" ", objects.Select(o => o.ToString()).ToArray());
            MessageType type;
            if (line.ToLower().Contains("error") || line.ToLower().Contains("exception"))
            {
                type = MessageType.Error;
            }
            else if (line.ToLower().Contains("warning") || line.ToLower().Contains("disabled"))
            {
                type = MessageType.Warning;
            }
            else if (line.ToLower().Contains("success"))
            {
                type = MessageType.Success;
            }
            else
            {
                type = MessageType.Message;
            }
            var senderName = Manifest.Name;
            var senderFile = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().DeclaringType.Name;
            WriteLine(senderName, senderFile, type, line);
        }

        public override void WriteLine(string line, MessageType type = MessageType.Message)
        {
            Logger?.Log(line);
            CallWriteCallback(Manifest, line);

            var message = new SocketMessage
            {
                SenderName = Manifest.Name,
                SenderType = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().DeclaringType.Name,
                Type = type,
                Message = line
            };
            var json = JsonConvert.SerializeObject(message);

            WriteToSocket(json);
        }

        private void WriteLine(string senderName, string senderFile, MessageType type, string line)
        {
            Logger?.Log(line);
            CallWriteCallback(Manifest, line);

            var message = new SocketMessage
            {
                SenderName = senderName,
                SenderType = senderFile,
                Type = type,
                Message = line
            };
            var json = JsonConvert.SerializeObject(message);

            WriteToSocket(json);
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
            var bytes = Encoding.UTF8.GetBytes(message);
            _socket?.Send(bytes);
        }
    }
}