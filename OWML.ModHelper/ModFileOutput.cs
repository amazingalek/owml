﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using OWML.Common;

namespace OWML.ModHelper
{
    public class ModFileOutput : ModConsole
    {
        private static FileStream _writer;

        public ModFileOutput(IOwmlConfig config, IModLogger logger, IModManifest manifest) : base(config, logger, manifest)
        {
            if (_writer == null)
            {
                _writer = File.Open(config.OutputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            }
        }

        public override void WriteLine(string s)
        {
            Logger.Log(s);
            CallWriteCallback(Manifest, s);
            var message = $"[{Manifest.Name}]: {s}";
            InternalWriteLine(message);
        }

        public override void WriteLine(params object[] objects)
        {
            WriteLine(string.Join(" ", objects.Select(o => o.ToString()).ToArray()));
        }

        private static void InternalWriteLine(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            _writer.Write(bytes, 0, bytes.Length);
            _writer.Flush();
        }

    }
}
