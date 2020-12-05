﻿using OWML.Common;
using OWML.Common.Interfaces;
using OWML.Common.Models;
using UnityEngine;

namespace OWML.ModHelper.Menus
{
    public class OwmlConfigMenu : ModConfigMenuBase
    {
        private const string BlockInputTitle = "Mod inputs can block game actions";

        private readonly string _defaultConfigPath = $"{Application.dataPath}/Managed/{Constants.OwmlDefaultConfigFileName}";

        private readonly IOwmlConfig _config;
        private readonly IOwmlConfig _defaultConfig;

        public OwmlConfigMenu(IModManifest manifest, IOwmlConfig config, IModStorage storage) 
            : base(manifest, storage)
        {
            _config = config;
            _defaultConfig = JsonHelper.LoadJsonObject<OwmlConfig>(_defaultConfigPath);
        }

        protected override void AddInputs()
        {
            var index = 2;
            AddConfigInput(BlockInputTitle, _config.BlockInput, index++);
            UpdateNavigation();
            SelectFirst();
        }

        protected override void UpdateUIValues()
        {
            GetToggleInput(BlockInputTitle).Value = _config.BlockInput;
        }

        protected override void OnSave()
        {
            _config.BlockInput = (bool)GetInputValue(BlockInputTitle);
            Storage.Save(_config, Constants.OwmlConfigFileName);
            Close();
        }

        protected override void OnReset()
        {
            _config.GamePath = _defaultConfig.GamePath;
            _config.BlockInput = _defaultConfig.BlockInput;
            UpdateUIValues();
        }

    }
}
