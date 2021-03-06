﻿using OWML.Common;
using OWML.Common.Menus;
using OWML.Utils;
using UnityEngine.UI;
using UnityEngine;

namespace OWML.ModHelper.Menus
{
	public class ModPromptButton : ModTitleButton, IModPromptButton
	{
		private ScreenPrompt _prompt;
		private readonly UITextType _textId;
		private readonly ButtonWithHotkeyImageElement _hotkeyButton;

		public string DefaultTitle => UITextLibrary.GetString(_textId);

		public ScreenPrompt Prompt
		{
			get => _prompt;
			set
			{
				_prompt = value;
				_hotkeyButton.SetPrompt(value);
				if (_prompt.GetText() != DefaultTitle)
				{
					GameObject.Destroy(Button.GetComponentInChildren<LocalizedText>());
				}
			}
		}

		public override string Title
		{
			set
			{
				if (_prompt == null)
				{
					Prompt = new ScreenPrompt(value);
					return;
				}
				_prompt.SetText(value);
				if (value != DefaultTitle)
				{
					GameObject.Destroy(Button.GetComponentInChildren<LocalizedText>());
				}
			}
		}

		public ModPromptButton(Button button, IModMenu menu, IModConsole console)
			: base(button, menu)
		{
			_hotkeyButton = Button.GetComponent<ButtonWithHotkeyImageElement>();
			if (_hotkeyButton == null)
			{
				console.WriteLine("Error - Can't setup ModPromptButton for this button.", MessageType.Error);
				return;
			}
			_prompt = _hotkeyButton.GetValue<ScreenPrompt>("_screenPrompt");
			_textId = Button.GetComponentInChildren<LocalizedText>(true)?.GetValue<UITextType>("_textID") ?? UITextType.None;
		}
	}
}
