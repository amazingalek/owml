﻿using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using OWML.ModHelper.Menus;
using OWML.ModHelper.Input;
using UnityEngine;
using System;
using OWML.Common.Enums;
using OWML.Common.Models;
using OWML.Logging;

namespace OWML.ModLoader
{
    public class ModLoader
    {
        private static readonly string ConfigPath = $"{Application.dataPath}/Managed/{Constants.OwmlConfigFileName}";

        private static readonly string DefaultConfigPath = $"{Application.dataPath}/Managed/{Constants.OwmlDefaultConfigFileName}";

        private static readonly string ManifestPath = $"{Application.dataPath}/Managed/{Constants.OwmlManifestFileName}";

        public static void LoadMods()
        {
            var owmlGo = new GameObject();
            owmlGo.AddComponent<OwmlBehaviour>();

            var owmlConfig = JsonHelper.LoadJsonObject<OwmlConfig>(ConfigPath);
            var owmlDefaultConfig = JsonHelper.LoadJsonObject<OwmlConfig>(DefaultConfigPath);
            var owmlManifest = JsonHelper.LoadJsonObject<ModManifest>(ManifestPath);
            if (owmlConfig == null || owmlManifest == null)
            {
                // Everything is wrong and can't write to console...
                return;
            }

            var startTime = DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");
            var logFileName = $"{owmlConfig.OWMLPath}Logs/OWML.Log.{startTime}.txt";
            var logger = new ModLogger(owmlConfig, owmlManifest, logFileName);
            logger.Log("Got config!");

            var socket = new ModSocket(owmlConfig.SocketPort);
            var unityLogger = new UnityLogger(socket);
            var console = new ModSocketOutput(owmlConfig, logger, owmlManifest, socket);
            console.WriteLine("Mod loader has been initialized.");
            console.WriteLine($"For detailed log, see Logs/OWML.Log.{startTime}.txt");
            console.WriteLine($"Game version: {Application.version}", MessageType.Info);

            var modSorter = new ModSorter(console);
            var modFinder = new ModFinder(owmlConfig, console);
            var harmonyHelper = new HarmonyHelper(logger, console);
            var events = new ModEvents(logger, console, harmonyHelper);
            var inputHandler = new ModInputHandler(logger, console, harmonyHelper, owmlConfig, events);
            var owmlStorage = new ModStorage(owmlManifest);
            var owmlMenu = new OwmlConfigMenu(owmlManifest, owmlConfig, owmlDefaultConfig, owmlStorage);
            var menus = new ModMenus(events, inputHandler, owmlMenu, owmlStorage);
            var owo = new Owo(modFinder, logger, owmlConfig, menus, harmonyHelper,
                inputHandler, modSorter, unityLogger, socket, logFileName);

            owo.LoadMods();
        }
    }
}