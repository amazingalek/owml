﻿namespace OWML.Common.Menus
{
	public interface IModPopupManager
	{
		void Initialize(PopupInputMenu inputMenu);

		IModMessagePopup CreateMessagePopup(string message, bool addCancel = false, string okMessage = "OK", string cancelMessage = "Cancel");

		IModInputMenu CreateInputPopup(InputType inputType, string value);

		IModInputCombinationElementMenu CreateCombinationInput(string value, string comboName,
			IModInputCombinationMenu combinationMenu = null, IModInputCombinationElement element = null);
	}
}
