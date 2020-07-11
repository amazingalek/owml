﻿using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace OWML.ModHelper.Menus
{
    public class ModInputCombinationMenu : ModPopupMenu, IModInputCombinationMenu
    {
        public event Action<string> OnConfirm;
        public event Action OnCancel;

        public List<IModInputCombinationElement> CombinationElements { get; }

        private List<Selectable> _selectables;
        private IModInputCombinationElement _combinationElementTemplate;

        public ModInputCombinationMenu(IModConsole console) : base(console)
        {
            CombinationElements = new List<IModInputCombinationElement>();
        }

        public string GenerateCombination()
        {
            var toDestroy = CombinationElements.Where(x => x.Title == "").ToList();
            toDestroy.ForEach(c => c.DestroySelf());
            return string.Join("/", CombinationElements.Select(x => x.Title).ToArray());
        }

        public void FillMenu(string combination)
        {
            _selectables = new List<Selectable>();
            CombinationElements.ForEach(element => element.Destroy());
            CombinationElements.Clear();
            combination.Split('/').ToList().ForEach(AddCombinationElement);
            SelectFirst();
            UpdateNavigation(_selectables);
        }

        public void Initialize(Menu menu, IModInputCombinationElement combinationElementTemplate)
        {
            _combinationElementTemplate = combinationElementTemplate;

            var blocker = menu.GetComponentsInChildren<GraphicRaycaster>(true)
                .Single(x => x.name == "RebindingModeBlocker");
            blocker.gameObject.SetActive(false);

            var labelPanel = menu.GetValue<GameObject>("_selectableItemsRoot")
                .GetComponentInChildren<HorizontalLayoutGroup>(true);
            labelPanel.gameObject.SetActive(false);

            var layoutGroup = menu.GetComponentsInChildren<VerticalLayoutGroup>(true)
                .Single(x => x.name == "Content");
            Initialize(menu, layoutGroup);

            var saveButton = GetButton("UIElement-SaveAndExit");
            var addButton = GetButton("UIElement-ResetToDefaultsButton");
            var cancelButton = GetButton("UIElement-DiscardChangesButton");

            if (saveButton == null || addButton == null || cancelButton == null)
            {
                Console.WriteLine("Error: Failed to setup combination menu");
                return;
            }

            saveButton.OnClick += OnSave;
            addButton.OnClick += OnAdd;
            cancelButton.OnClick += OnExit;

            saveButton.SetControllerCommand(InputLibrary.confirm);
            cancelButton.SetControllerCommand(InputLibrary.cancel);
            addButton.SetControllerCommand(InputLibrary.setDefaults);

            var localText = addButton.Button.gameObject
                .GetComponentInChildren<LocalizedText>(true);
            if (localText != null)
            {
                GameObject.Destroy(localText);
            }
            var buttonWithHotkey = addButton.Button.gameObject
                .GetComponentInChildren<ButtonWithHotkeyImageElement>(true);
            if (buttonWithHotkey != null)
            {
                buttonWithHotkey.SetPrompt(new ScreenPrompt(InputLibrary.setDefaults, "Add Alternative"));
            }

            Title = "Edit Combination";

            GetButton("UIElement-CancelOutOfRebinding")?.Hide();
            GetButton("UIElement-KeyRebinder")?.Hide();

            foreach (Transform child in layoutGroup.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void RemoveFromNavigation(int index)
        {
            var upIndex = (index - 1 + _selectables.Count) % _selectables.Count;
            var downIndex = (index + 1) % _selectables.Count;
            var navigation = _selectables[upIndex].navigation;
            navigation.selectOnDown = _selectables[downIndex];
            _selectables[upIndex].navigation = navigation;
            navigation = _selectables[downIndex].navigation;
            navigation.selectOnUp = _selectables[upIndex];
            _selectables[downIndex].navigation = navigation;
            if (downIndex == 0)
            {
                _selectables[upIndex].Select();
            }
            else
            {
                _selectables[downIndex].Select();
            }
            _selectables.RemoveAt(index);
        }

        private void AddToNavigation(int index)
        {
            var current = _selectables[index];
            var next = _selectables[(index + 1) % _selectables.Count];
            var previous = _selectables[(_selectables.Count - 2 + _selectables.Count) % _selectables.Count];

            var navigation = next.navigation;
            navigation.selectOnUp = current;
            next.navigation = navigation;

            navigation = previous.navigation;
            navigation.selectOnDown = current;
            previous.navigation = navigation;

            navigation = current.navigation;
            navigation.selectOnDown = next;
            navigation.selectOnUp = previous;
            current.navigation = navigation;
        }

        public void RemoveCombinationElement(IModInputCombinationElement element)
        {
            CombinationElements.Remove(element);
            var selectable = element.Toggle.GetComponent<Selectable>();
            var index = _selectables.IndexOf(selectable);
            RemoveFromNavigation(index);
        }

        private void AddCombinationElement(string combination)
        {
            AddCombinationElement(combination, CombinationElements.Count);
        }

        private void AddCombinationElement(string combination, int index)
        {
            var element = _combinationElementTemplate.Copy(combination);
            var transform = element.Toggle.transform;
            var scale = transform.localScale;
            transform.parent = Layout.transform;
            element.Index = index;
            element.Initialize(this);
            CombinationElements.Add(element);
            element.Toggle.transform.localScale = scale;
            _selectables.Add(element.Toggle.GetComponent<Selectable>());
        }

        private void OnSave()
        {
            OnConfirm?.Invoke(GenerateCombination());
            Close();
        }

        private void OnExit()
        {
            OnCancel?.Invoke();
            Close();
        }

        private void OnAdd()
        {
            AddCombinationElement("");
            AddToNavigation(_selectables.Count - 1);
            Locator.GetMenuInputModule().SelectOnNextUpdate(_selectables[_selectables.Count - 1]);
        }
    }
}