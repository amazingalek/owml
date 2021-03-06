﻿using System.Collections.Generic;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OWML.ModHelper.Menus
{
	public class ModInputCombinationElement : ModToggleInput, IModInputCombinationElement
	{
		public IModLayoutManager Layout { get; }

		public override string Title
		{
			get => _combination;
			set
			{
				_combination = value;
				UpdateContents();
			}
		}

		private string _combination;
		private readonly IModInputHandler _inputHandler;
		private readonly IModPopupManager _popupManager;
		private readonly IModConsole _console;

		private readonly List<SingleAxisCommand> _openCommands = new()
		{
			InputLibrary.menuConfirm
		};


		public ModInputCombinationElement(
			TwoButtonToggleElement toggle, 
			IModMenu menu,
			IModPopupManager popupManager,
			IModInputHandler inputHandler,
			IModConsole console,
			string combination = "") 
				: base(toggle, menu)
		{
			_inputHandler = inputHandler;
			_console = console;
			_combination = combination;
			Initialize(menu);
			SetupButtons();
			var scale = toggle.transform.localScale;

			var layoutObject = toggle.GetComponentInChildren<HorizontalLayoutGroup>(true).transform.Find("LabelBlock")?
									 .GetComponentInChildren<HorizontalLayoutGroup>(true).gameObject;
			if (layoutObject == null)
			{
				_console.WriteLine("Error - Failed to setup an element for combination editor.", MessageType.Error);
				return;
			}
			var layoutGroup = layoutObject.GetComponent<HorizontalLayoutGroup>();

			layoutGroup.childControlWidth = false;
			layoutGroup.childControlHeight = false;
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.childForceExpandWidth = false;
			layoutGroup.spacing = 0f;
			var constantGraphics = layoutObject.GetComponentsInChildren<Graphic>(true);
			layoutObject.transform.GetComponentInChildren<Text>(true).gameObject.SetActive(false);
			var styleManager = GameObject.FindObjectOfType<UIStyleManager>();
			var styleApplier = ModUIStyleApplier.ReplaceStyleApplier(toggle.gameObject);
			Layout = new ModLayoutManager(layoutGroup, styleManager, styleApplier, scale, constantGraphics);
			UpdateContents();
			_popupManager = popupManager;
		}

		private void SetupButtons()
		{
			var commandObject = new GameObject();
			var commandComponent = commandObject.AddComponent<ModCommandListener>();
			_openCommands.ForEach(commandComponent.AddToListener);
			commandComponent.OnNewlyReleased += OnEditButton;
			commandComponent.BlockNextRelease();
			YesButton.Title = "Edit";
			YesButton.OnClick += OnEditClick;

			var deleteCommand = new SingleAxisCommand();
			var deleteBindingGamepad = new InputBinding(JoystickButton.FaceLeft);
			var deleteBindingKeyboard = new InputBinding(KeyCode.Delete);
			deleteCommand.SetInputs(deleteBindingGamepad, deleteBindingKeyboard);
			commandObject = new GameObject();
			var updater = commandObject.AddComponent<ModCommandUpdater>();
			updater.Initialize(deleteCommand);
			commandComponent = commandObject.AddComponent<ModCommandListener>();
			commandComponent.AddToListener(deleteCommand);
			commandComponent.OnNewlyReleased += OnDeleteButton;
			commandComponent.BlockNextRelease();
			NoButton.Title = "Delete";
			NoButton.OnClick += OnDeleteClick;
		}

		private void UpdateContents()
		{
			Layout.Clear();
			if (_combination != "")
			{
				var keyStrings = _combination.Split('+');
				for (var j = 0; j < keyStrings.Length; j++)
				{
					AddKeySign(keyStrings[j]);
					if (j < keyStrings.Length - 1)
					{
						Layout.AddTextAt("+", Layout.ChildCount - 1);
					}
				}
			}
			Layout.UpdateState();
		}

		private void AddKeySign(string key)
		{
			Layout.AddPictureAt(
			   _inputHandler.Textures.KeyTexture(key),
			   Layout.ChildCount - 1, ModInputLibrary.ScaleDown);
		}

		private void OnEditButton(SingleAxisCommand command)
		{
			if (IsSelected)
			{
				OnEditClick();
			}
		}

		private void OnEditClick()
		{
			EventSystem.current.SetSelectedGameObject(Toggle.gameObject); // make sure it gets selected after popup closes
			var name = Menu is IModInputCombinationMenu menu ? menu.Title : "";

			var popup = _popupManager.CreateCombinationInput(_combination, name, Menu as IModInputCombinationMenu, this);
			popup.OnConfirm += OnPopupMenuConfirm;
		}

		private void OnPopupMenuConfirm(string combination)
		{
			_combination = combination;
			UpdateContents();
		}

		public void Destroy()
		{
			if (Toggle == null)
			{
				return;
			}
			Layout.Clear();
			Layout.UpdateState();
			Title = "";
			Toggle.gameObject.SetActive(false);
			GameObject.Destroy(Toggle.gameObject);
		}

		public void DestroySelf()
		{
			Destroy();
			(Menu as IModInputCombinationMenu)?.RemoveCombinationElement(this);
		}

		private void OnDeleteButton(SingleAxisCommand command)
		{
			if (IsSelected)
			{
				OnDeleteClick();
			}
		}

		private void OnDeleteClick()
		{
			DestroySelf();
		}

		public new IModInputCombinationElement Copy()
		{
			return Copy(_combination);
		}

		public new IModInputCombinationElement Copy(string combination)
		{
			var copy = GameObject.Instantiate(Toggle);
			GameObject.Destroy(copy.GetComponentInChildren<LocalizedText>(true));
			return new ModInputCombinationElement(copy, Menu, _popupManager, _inputHandler, _console, combination);
		}
	}
}
