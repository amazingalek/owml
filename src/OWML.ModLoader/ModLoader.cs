﻿using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using OWML.ModHelper.Menus;
using OWML.ModHelper.Input;
using OWML.Logging;
using OWML.ModHelper.Assets;
using OWML.Abstractions;
using OWML.Common.Menus;
using OWML.Utils;
using UnityEngine;

namespace OWML.ModLoader
{
	public class ModLoader
	{
		public static void LoadMods()
		{
			var appHelper = new ApplicationHelper();
			var goHelper = new GameObjectHelper();
			var container = CreateContainer(appHelper, goHelper);

			var owo = container.Resolve<Owo>();
			owo.LoadMods();
		}

		public static Container CreateContainer(IApplicationHelper appHelper, IGameObjectHelper goHelper)
		{
			var owmlConfig = JsonHelper.LoadJsonObject<OwmlConfig>($"{appHelper.DataPath}/Managed/{Constants.OwmlConfigFileName}");
			var owmlManifest = JsonHelper.LoadJsonObject<ModManifest>($"{appHelper.DataPath}/Managed/{Constants.OwmlManifestFileName}");

			if (owmlConfig == null || owmlManifest == null)
			{
				throw new UnityException("Can't load OWML config or manifest.");
			}

			var bindingChangeListener = goHelper.CreateAndAdd<IBindingChangeListener, BindingChangeListener>("GameBindingsChangeListener");
			var unityEvents = goHelper.CreateAndAdd<IModUnityEvents, ModUnityEvents>();

			return new Container()
				.Add(appHelper)
				.Add(goHelper)
				.Add(bindingChangeListener)
				.Add(unityEvents)
				.Add<IOwmlConfig>(owmlConfig)
				.Add<IModManifest>(owmlManifest)
				.Add<IModLogger, ModLogger>()
				.Add<IModSocket, ModSocket>()
				.Add<IUnityLogger, UnityLogger>()
				.Add<IModConsole, ModSocketOutput>()
				.Add<IModSorter, ModSorter>()
				.Add<IModFinder, ModFinder>()
				.Add<IHarmonyHelper, HarmonyHelper>()
				.Add<IModPlayerEvents, ModPlayerEvents>()
				.Add<IModSceneEvents, ModSceneEvents>()
				.Add<IModEvents, ModEvents>()
				.Add<IModInputHandler, ModInputHandler>()
				.Add<IModStorage, ModStorage>()
				.Add<IModConfigMenuBase, OwmlConfigMenu>()
				.Add<IModTabbedMenu, ModOptionsMenu>()
				.Add<IModMainMenu, ModMainMenu>()
				.Add<IModPauseMenu, ModPauseMenu>()
				.Add<IModsMenu, ModsMenu>()
				.Add<IModInputMenu, ModInputMenu>()
				.Add<IModInputTextures, ModInputTextures>()
				.Add<IModMessagePopup, ModMessagePopup>()
				.Add<IModInputCombinationElementMenu, ModInputCombinationElementMenu>()
				.Add<IModPopupManager, ModPopupManager>()
				.Add<IModInputCombinationMenu, ModInputCombinationMenu>()
				.Add<IModMenus, ModMenus>()
				.Add<IObjImporter, ObjImporter>()
				.Add<IProcessHelper, ProcessHelper>()
				.Add<Owo>();
		}
	}
}