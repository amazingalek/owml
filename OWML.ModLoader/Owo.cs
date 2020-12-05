﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OWML.Common.Enums;
using OWML.Common.Interfaces;
using OWML.Common.Interfaces.Menus;
using OWML.Logging;
using OWML.ModHelper;
using OWML.ModHelper.Assets;
using OWML.ModHelper.Events;
using OWML.ModHelper.Interaction;
using UnityEngine;

namespace OWML.ModLoader
{
    internal class Owo
    {
        private readonly IModFinder _modFinder;
        private readonly IModLogger _logger;
        private readonly IOwmlConfig _owmlConfig;
        private readonly IModMenus _menus;
        private readonly IHarmonyHelper _harmonyHelper;
        private readonly IModInputHandler _inputHandler;
        private readonly IModSorter _sorter;
        private readonly IUnityLogger _unityLogger;
        private readonly IModSocket _socket;

        private readonly string _logFileName;
        private readonly IList<IModBehaviour> _modList = new List<IModBehaviour>();

        public Owo(
            IModFinder modFinder, 
            IModLogger logger,
            IOwmlConfig owmlConfig, 
            IModMenus menus, 
            IHarmonyHelper harmonyHelper,
            IModInputHandler inputHandler,
            IModSorter sorter,
            IUnityLogger unityLogger, 
            IModSocket socket,
            string logFileName)
        {
            _modFinder = modFinder;
            _logger = logger;
            _owmlConfig = owmlConfig;
            _menus = menus;
            _harmonyHelper = harmonyHelper;
            _inputHandler = inputHandler;
            _sorter = sorter;
            _unityLogger = unityLogger;
            _socket = socket;
            _logFileName = logFileName;
        }

        public void LoadMods()
        {
            _unityLogger.Start();

            var mods = _modFinder.GetMods();
            var changedSettings = mods.Where(mod => mod.FixConfigs()).Select(mod => mod.Manifest.Name).ToArray();
            if (changedSettings.Any())
            {
                ModConsole.OwmlConsole.WriteLine("Warning - Settings of following mods changed:\n\t" + string.Join("\n\t", changedSettings),
                    MessageType.Warning);
            }

            var normalMods = mods.Where(mod => !mod.Manifest.PriorityLoad).ToList();
            var sortedNormal = _sorter.SortMods(normalMods);

            var priorityMods = mods.Where(mod => mod.Manifest.PriorityLoad).ToList();
            var sortedPriority = _sorter.SortMods(priorityMods);

            var modNames = mods.Where(mod => mod.Config.Enabled)
                .Select(mod => mod.Manifest.UniqueName).ToList();
            var sortedMods = sortedPriority.Concat(sortedNormal);

            foreach (var modData in sortedMods)
            {
                var missingDependencies = modData.Config.Enabled ?
                    modData.Manifest.Dependencies.Where(dependency => !modNames.Contains(dependency)).ToList() :
                    new List<string>();
                missingDependencies.ForEach(dependency => ModConsole.OwmlConsole.WriteLine(
                    $"Error! {modData.Manifest.UniqueName} needs {dependency}, but it's disabled/missing!",
                    MessageType.Error));
                var modType = LoadMod(modData);
                if (modType == null || missingDependencies.Any())
                {
                    _menus.ModsMenu.AddMod(modData, null);
                    continue;
                }
                var helper = CreateModHelper(modData);
                var initMod = InitializeMod(modType, helper);
                _menus.ModsMenu.AddMod(modData, initMod);
                _modList.Add(initMod);
            }
        }

        private Type LoadMod(IModData modData)
        {
            if (!modData.Config.Enabled)
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
                ModConsole.OwmlConsole.WriteLine($"Error while trying to get {typeof(ModBehaviour)}: {ex.Message}", MessageType.Error);
                return null;
            }
        }

        private IModHelper CreateModHelper(IModData modData)
        {
            var logger = new ModLogger(_owmlConfig, modData.Manifest, _logFileName);
            var console = new ModSocketOutput(_owmlConfig, logger, modData.Manifest, _socket);
            var assets = new ModAssets(console, modData.Manifest);
            var modStorage = new ModStorage(modData.Manifest);
            var events = new ModEvents(logger, console, _harmonyHelper);
            var interaction = new ModInteraction(_modList, new InterfaceProxyFactory(), modData.Manifest);
            return new ModHelper.ModHelper(logger, console, _harmonyHelper,
                events, assets, modStorage, _menus, modData.Manifest, modData.Config,
                _owmlConfig, _inputHandler, interaction);
        }

        private IModBehaviour InitializeMod(Type modType, IModHelper helper)
        {
            _logger.Log($"Initializing {helper.Manifest.UniqueName} ({helper.Manifest.Version})...");
            _logger.Log("Adding mod behaviour...");
            var go = new GameObject(helper.Manifest.UniqueName);
            try
            {
                var mod = (ModBehaviour)go.AddComponent(modType);
                _logger.Log("Added! Initializing...");
                mod.Init(helper);
                return mod;
            }
            catch (Exception ex)
            {
                ModConsole.OwmlConsole.WriteLine($"Error while adding/initializing {helper.Manifest.UniqueName}: {ex}", MessageType.Error);
                return null;
            }
        }

    }
}
