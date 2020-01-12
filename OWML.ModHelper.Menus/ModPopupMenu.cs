﻿using System;
using OWML.Common;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace OWML.ModHelper.Menus
{
    public class ModPopupMenu : ModMenu, IModPopupMenu
    {
        public Action OnOpen { get; set; }
        public Action OnClose { get; set; }
        public Action OnInit { get; set; }

        public bool IsOpen { get; private set; }

        public string Title
        {
            get => _title.text;
            set => _title.text = value;
        }

        private readonly IModLogger _logger;
        private readonly IModConsole _console;

        private Text _title;

        public ModPopupMenu(IModLogger logger, IModConsole console) : base(logger, console)
        {
            _logger = logger;
            _console = console;
        }

        public override void Initialize(Menu menu)
        {
            base.Initialize(menu);
            _title = Menu.GetComponentInChildren<Text>();
            var localizedText = _title.GetComponent<LocalizedText>();
            if (localizedText != null)
            {
                Title = UITextLibrary.GetString(localizedText.GetValue<UITextType>("_textID"));
                GameObject.Destroy(localizedText);
            }
            Menu.OnActivateMenu += OnActivateMenu;
            Menu.OnDeactivateMenu += OnDeactivateMenu;
            OnInit?.Invoke();
        }

        private void OnDeactivateMenu()
        {
            IsOpen = false;
            OnClose?.Invoke();
        }

        private void OnActivateMenu()
        {
            IsOpen = true;
            OnOpen?.Invoke();
        }

        public void Open()
        {
            if (Menu == null)
            {
                _console.WriteLine("Warning: can't open menu, it doesn't exist.");
                return;
            }
            Menu.EnableMenu(true);
        }

        public void Close()
        {
            if (Menu == null)
            {
                _console.WriteLine("Warning: can't close menu, it doesn't exist.");
                return;
            }
            Menu.Deactivate();
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public IModPopupMenu Copy()
        {
            if (Menu == null)
            {
                _console.WriteLine("Warning: can't copy menu, it doesn't exist.");
                return null;
            }
            var menu = GameObject.Instantiate(Menu, Menu.transform.parent);
            var modMenu = new ModPopupMenu(_logger, _console);
            modMenu.Initialize(menu);
            return modMenu;
        }

        [Obsolete("Use Copy and Title instead")]
        public IModPopupMenu CreateCopy(string title)
        {
            if (Menu == null)
            {
                _console.WriteLine("Warning: can't copy menu, it doesn't exist.");
                return null;
            }
            var menu = Copy();
            menu.Title = title;
            return menu;
        }

    }
}