﻿using System;
using System.IO;
using Newtonsoft.Json;
using OWML.Common;
using OWML.ModHelper;
using OWML.ModLoader;
using OWML.Patcher;

namespace OWML.Launcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            var owmlConfig = GetOwmlConfig();
            var writer = new OutputWriter();
            var modFinder = new ModFinder(owmlConfig, writer);
            var outputListener = new OutputListener(owmlConfig);
            var pathFinder = new PathFinder(owmlConfig, writer);
            var patcher = new ModPatcher(owmlConfig, writer);
            var app = new App(owmlConfig, writer, modFinder, outputListener, pathFinder, patcher);
            app.Run();
        }

        private static IOwmlConfig GetOwmlConfig()
        {
            var json = File.ReadAllText("OWML.Config.json")
                .Replace("\\", "/");
            var config = JsonConvert.DeserializeObject<OwmlConfig>(json);
            config.OWMLPath = AppDomain.CurrentDomain.BaseDirectory;
            return config;
        }

    }
}
