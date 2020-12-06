﻿using System.Diagnostics;
using OWML.Common.Interfaces;

namespace OWML.Launcher
{
    public class ProcessHelper : IProcessHelper
    {
        public void Start(string path, string[] args)
        {
            Process.Start(path, string.Join(" ", args));
        }

        public void KillCurrentProcess()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
