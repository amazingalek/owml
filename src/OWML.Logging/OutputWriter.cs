﻿using System;
using System.Linq;
using OWML.Common.Enums;
using OWML.Common.Interfaces;

namespace OWML.Logging
{
    public class OutputWriter : IModConsole
    {
        [Obsolete("Use WriteLine(string) or WriteLine(string, MessageType) instead.")]
        public void WriteLine(params object[] objects)
        {
            WriteLine(string.Join(" ", objects.Select(o => o.ToString()).ToArray()));
        }

        public void WriteLine(string line)
        {
            WriteLine(line, MessageType.Message);
        }

        public void WriteLine(string line, MessageType type)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            ConsoleUtils.WriteByType(type, line);
        }
    }
}