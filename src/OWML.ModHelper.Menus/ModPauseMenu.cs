﻿using OWML.Common.Interfaces.Menus;
using OWML.ModHelper.Events;

namespace OWML.ModHelper.Menus
{
    public class ModPauseMenu : ModPopupMenu, IModPauseMenu
    {
        public IModTabbedMenu OptionsMenu { get; } = new ModOptionsMenu(); // todo DI?

        public IModButton ResumeButton { get; private set; }

        public IModButton OptionsButton { get; private set; }

        public IModButton QuitButton { get; private set; }

        public void Initialize(SettingsManager settingsManager)
        {
            var pauseMenuManager = settingsManager.GetComponent<PauseMenuManager>();
            var optionsMenu = settingsManager.GetValue<TabbedMenu>("_mainSettingsMenu");
            OptionsMenu.Initialize(optionsMenu);

            var pauseMenu = pauseMenuManager.GetValue<Menu>("_pauseMenu");
            base.Initialize(pauseMenu);

            ResumeButton = GetTitleButton("Button-Unpause");
            OptionsButton = GetTitleButton("Button-Options");
            QuitButton = GetTitleButton("Button-ExitToMainMenu");

            InvokeOnInit();
        }

    }
}