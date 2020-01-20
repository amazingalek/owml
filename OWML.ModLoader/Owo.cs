﻿using System;
using System.Linq;
using System.Reflection;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using OWML.ModHelper.Assets;
using OWML.ModHelper.Events;
using OWML.ModHelper.Menus;
using UnityEngine;

namespace OWML.ModLoader
{
    internal class Owo
    {
        private readonly IModFinder _modFinder;
        private readonly IModLogger _logger;
        private readonly IModConsole _console;
        private readonly IOwmlConfig _owmlConfig;
        private readonly IModMenus _menus;
        private readonly IHarmonyHelper _harmonyHelper;

        public Owo(IModFinder modFinder, IModLogger logger, IModConsole console,
            IOwmlConfig owmlConfig, IModMenus menus, IHarmonyHelper harmonyHelper)
        {
            _modFinder = modFinder;
            _logger = logger;
            _console = console;
            _owmlConfig = owmlConfig;
            _menus = menus;
            _harmonyHelper = harmonyHelper;
        }

        public void LoadMods()
        {
            if (_owmlConfig.Verbose)
            {
                _console.WriteLine("Verbose mode is enabled");
                Application.logMessageReceived += OnLogMessageReceived;
            }
            var mods = _modFinder.GetMods();
            foreach (var modData in mods)
            {
                var modType = LoadMod(modData);
                if (modType == null)
                {
                    _logger.Log("Mod type is null, skipping");
                    continue;
                }
                var helper = CreateModHelper(modData);
                InitializeMod(modType, helper);
            }
        }

        private void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
            _logger.Log($"Unity log message ({type}): {message}. Stack trace: {stackTrace?.Trim()}");
            if (type == LogType.Error || type == LogType.Exception)
            {
                _console.WriteLine($"Unity log message: {message}. Stack trace: {stackTrace?.Trim()}");
            }
        }

        private ModsMenu CreateModMenu()
        {
            _console.WriteLine("Creating mod menu");
            var modsMenu = new ModsMenu(_logger, _console);
            var modMenuButton = _menus.MainMenu.AddButton("MODS", 3);
            modMenuButton.onClick.AddListener(() =>
            {
                modsMenu.Open();
            });
            return modsMenu;
        }

        private Type LoadMod(IModData modData)
        {
            var enabled = modData.Config.Enabled && modData.Manifest.Enabled;
            if (!enabled)
            {
                _logger.Log($"{modData.Manifest.UniqueName} is disabled");
                return null;
            }
            _logger.Log("Loading assembly: " + modData.Manifest.AssemblyPath);
            var assembly = Assembly.LoadFile(modData.Manifest.AssemblyPath);
            _logger.Log($"Loaded {assembly.FullName}");
            try
            {
                return assembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(ModBehaviour)));
            }
            catch (Exception ex)
            {
                _console.WriteLine($"Error while trying to get {typeof(ModBehaviour)}: {ex.Message}");
                return null;
            }
        }

        private IModHelper CreateModHelper(IModData modData)
        {
            var assets = new ModAssets(_console, modData.Manifest);
            var storage = new ModStorage(_console, modData.Manifest);
            var events = new ModEvents(_logger, _console, _harmonyHelper);
            return new ModHelper.ModHelper(_logger, _console, _harmonyHelper,
                events, assets, storage, _menus, modData.Manifest, modData.Config, _owmlConfig);
        }

        private void InitializeMod(Type modType, IModHelper helper)
        {
            _logger.Log($"Initializing {helper.Manifest.UniqueName} ({helper.Manifest.Version})...");
            _logger.Log("Adding mod behaviour...");
            var go = new GameObject(helper.Manifest.UniqueName);
            try
            {
                var mod = (ModBehaviour)go.AddComponent(modType);
                _logger.Log("Added! Initializing...");
                mod.Init(helper);
            }
            catch (Exception ex)
            {
                _console.WriteLine($"Error while adding/initializing {helper.Manifest.UniqueName}: {ex}");
                return;
            }
            _console.WriteLine($"Loaded {helper.Manifest.UniqueName} ({helper.Manifest.Version}).");
        }

    }
}
