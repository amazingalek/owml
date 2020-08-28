﻿using OWML.Common;
using System;

namespace OWML.ModHelper.Menus
{
    public class OwmlConfigMenu : ModConfigMenuBase
    {
        private const string BlockInputTitle = "Mod inputs can block game actions";

        private readonly IOwmlConfig _config;
        private readonly IOwmlConfig _defaultConfig;

        public OwmlConfigMenu(IModManifest manifest, IOwmlConfig config, IOwmlConfig defaultConfig) : base(manifest)
        {
            _config = config;
            _defaultConfig = defaultConfig;
        }

        protected override void AddInputs()
        {
            var index = 2;
            // TODO take care of useless callback.
            AddConfigInput(BlockInputTitle, _config.BlockInput, index++, (object _) => { });
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
