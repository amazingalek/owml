﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OWML.Common;
using OWML.GameFinder;
using OWML.ModHelper;
using OWML.Patcher;

namespace OWML.Launcher
{
    public class App
    {
        private readonly IOwmlConfig _owmlConfig;
        private readonly IModManifest _owmlManifest;
        private readonly IModConsole _writer;
        private readonly IModFinder _modFinder;
        private readonly PathFinder _pathFinder;
        private readonly OWPatcher _owPatcher;
        private readonly VRPatcher _vrPatcher;

        public App(IOwmlConfig owmlConfig, IModManifest owmlManifest, IModConsole writer, IModFinder modFinder,
            PathFinder pathFinder, OWPatcher owPatcher, VRPatcher vrPatcher)
        {
            _owmlConfig = owmlConfig;
            _owmlManifest = owmlManifest;
            _writer = writer;
            _modFinder = modFinder;
            _pathFinder = pathFinder;
            _owPatcher = owPatcher;
            _vrPatcher = vrPatcher;
        }

        public void Run()
        {
            _writer.WriteLine(MessageType.Message, $"Started OWML v{_owmlManifest.Version}");
            _writer.WriteLine(MessageType.Message, "For detailed log, see Logs/OWML.Log.txt");

            LocateGamePath();

            CopyGameFiles();

            CreateLogsDirectory();


            var mods = _modFinder.GetMods();

            ShowModList(mods);

            PatchGame(mods);

            StartGame();

            var hasPortArgument = CommandLineArguments.HasArgument(Constants.ConsolePortArgument);
            if (hasPortArgument)
            {
                ExitConsole();
            }

            Console.ReadLine();
        }

        private void LocateGamePath()
        {
            var gamePath = _pathFinder.FindGamePath();
            _writer.WriteLine(MessageType.Success, "Game found in " + gamePath);
            if (gamePath != _owmlConfig.GamePath)
            {
                _owmlConfig.GamePath = gamePath;
                JsonHelper.SaveJsonObject(Constants.OwmlConfigFileName, _owmlConfig);
            }
        }

        private void CopyGameFiles()
        {
            var filesToCopy = new[] { "UnityEngine.CoreModule.dll", "Assembly-CSharp.dll" };
            foreach (var fileName in filesToCopy)
            {
                File.Copy($"{_owmlConfig.ManagedPath}/{fileName}", fileName, true);
            }
            _writer.WriteLine(MessageType.Message, "Game files copied.");
        }

        private void ShowModList(IList<IModData> mods)
        {
            if (!mods.Any())
            {
                _writer.WriteLine(MessageType.Warning, "Warning - found no mods.");
                return;
            }
            _writer.WriteLine(MessageType.Message, "Found mods:");
            foreach (var modData in mods)
            {
                var stateText = modData.Config.Enabled ? "" : "(disabled)";
                var type = modData.Config.Enabled ? MessageType.Message : MessageType.Warning;

                _writer.WriteLine(type, $"* {modData.Manifest.UniqueName} v{modData.Manifest.Version} {stateText}");

                if (!string.IsNullOrEmpty(modData.Manifest.OWMLVersion) && !IsMadeForSameOwmlMajorVersion(modData.Manifest))
                {
                    _writer.WriteLine(MessageType.Warning, $"  Warning - made for old version of OWML: v{modData.Manifest.OWMLVersion}");
                }
            }
        }

        private bool IsMadeForSameOwmlMajorVersion(IModManifest manifest)
        {
            var owmlVersionSplit = _owmlManifest.Version.Split('.');
            var modVersionSplit = manifest.OWMLVersion.Split('.');
            return owmlVersionSplit.Length == modVersionSplit.Length &&
                   owmlVersionSplit[0] == modVersionSplit[0] &&
                   owmlVersionSplit[1] == modVersionSplit[1];
        }

        private bool HasVrMod(IList<IModData> mods)
        {
            var vrMod = mods.FirstOrDefault(x => x.Config.RequireVR && x.Config.Enabled);
            var hasVrMod = vrMod != null;
            _writer.WriteLine(MessageType.Message, hasVrMod ? $"{vrMod.Manifest.UniqueName} requires VR." : "No mods require VR.");
            return hasVrMod;
        }

        private void PatchGame(IList<IModData> mods)
        {
            _owPatcher.PatchGame();

            try
            {
                var enableVR = HasVrMod(mods);
                _vrPatcher.PatchVR(enableVR);
            }
            catch (Exception ex)
            {
                _writer.WriteLine(MessageType.Error, $"Error while applying VR patch: {ex}");
            }
        }

        private void StartGame()
        {
            _writer.WriteLine(MessageType.Message, "Starting game...");
            try
            {
                Process.Start($"{_owmlConfig.GamePath}/OuterWilds.exe");
            }
            catch (Exception ex)
            {
                _writer.WriteLine(MessageType.Error, "Error while starting game: " + ex.Message);
            }
        }

        private void ExitConsole()
        {
            Environment.Exit(0);
        }

        private void CreateLogsDirectory()
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
        }
    }
}
