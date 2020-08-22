﻿using Newtonsoft.Json.Linq;
using OWML.Common;
using OWML.Common.Menus;
using System.Linq;
using OWML.Logging;
using System;

namespace OWML.ModHelper.Menus
{
    public abstract class ModConfigMenuBase : ModMenuWithSelectables, IModConfigMenuBase
    {
        public IModManifest Manifest { get; }

        protected readonly IModStorage Storage;

        private IModToggleInput _toggleTemplate;
        private IModSliderInput _sliderTemplate;
        private IModSelectorInput _selectorTemplate;
        private IModTextInput _textInputTemplate;
        private IModComboInput _comboInputTemplate;
        private IModNumberInput _numberInputTemplate;

        protected abstract void AddInputs();
        protected abstract void UpdateUIValues();

        protected ModConfigMenuBase(IModManifest manifest)
        {
            Manifest = manifest;
            Storage = new ModStorage(manifest);
        }

        public void Initialize(Menu menu, IModToggleInput toggleTemplate, IModSliderInput sliderTemplate,
            IModTextInput textInputTemplate, IModNumberInput numberInputTemplate,
            IModComboInput comboInputTemplate, IModSelectorInput selectorTemplate)
        {
            _toggleTemplate = toggleTemplate;
            _sliderTemplate = sliderTemplate;
            _textInputTemplate = textInputTemplate;
            _numberInputTemplate = numberInputTemplate;
            _comboInputTemplate = comboInputTemplate;
            _selectorTemplate = selectorTemplate;

            base.Initialize(menu);

            Title = Manifest.Name;

            AddInputs();
        }

        public override void Open()
        {
            base.Open();
            UpdateUIValues();
        }

        protected void AddConfigInput(string key, object value, int index, Action<object> onChange)
        {
            if (value is bool)
            {
                AddToggleInput(key, index, onChange);
                return;
            }

            if (value is string)
            {
                AddTextInput(key, index, onChange);
                return;
            }

            if (new[] { typeof(long), typeof(int), typeof(float), typeof(double) }.Contains(value.GetType()))
            {
                AddNumberInput(key, index, onChange);
                return;
            }

            if (value is JObject obj)
            {
                switch ((string)obj["type"])
                {
                    case "slider":
                        AddSliderInput(key, obj, index, onChange);
                        return;
                    case "toggle":
                        AddToggleInput(key, obj, index, onChange);
                        return;
                    case "selector":
                        AddSelectorInput(key, obj, index, onChange);
                        return;
                    case "input":
                        AddComboInput(key, index, onChange);
                        return;
                    default:
                        ModConsole.OwmlConsole.WriteLine("Error - Unrecognized complex setting: " + value, MessageType.Error);
                        return;
                }
            }

            ModConsole.OwmlConsole.WriteLine("Error - Unrecognized setting type: " + value.GetType(), MessageType.Error);
        }

        private void AddToggleInput(string key, int index, Action<object> onChange)
        {
            var toggle = AddToggleInput(_toggleTemplate.Copy(key), index);
            toggle.OnChange += value => onChange(value);
            toggle.YesButton.Title = "Yes";
            toggle.NoButton.Title = "No";
            toggle.Element.name = key;
            toggle.Title = key;
            toggle.Show();
        }

        private void AddToggleInput(string key, JObject obj, int index, Action<object> onChange)
        {
            var toggle = AddToggleInput(_toggleTemplate.Copy(key), index);
            toggle.OnChange += value => onChange(value);
            toggle.YesButton.Title = (string)obj["yes"];
            toggle.NoButton.Title = (string)obj["no"];
            toggle.Element.name = key;
            toggle.Title = (string)obj["title"] ?? key;
            toggle.Show();
        }

        private void AddSliderInput(string key, JObject obj, int index, Action<object> onChange)
        {
            var slider = AddSliderInput(_sliderTemplate.Copy(key), index);
            slider.OnChange += value => onChange(value);
            slider.Min = (float)obj["min"];
            slider.Max = (float)obj["max"];
            slider.Element.name = key;
            slider.Title = (string)obj["title"] ?? key;
            slider.Show();
        }

        private void AddSelectorInput(string key, JObject obj, int index, Action<object> onChange)
        {
            var options = obj["options"].ToObject<string[]>();
            var selector = AddSelectorInput(_selectorTemplate.Copy(key), index);
            selector.OnChange += value => onChange(value);
            selector.Element.name = key;
            selector.Title = (string)obj["title"] ?? key;
            selector.Initialize((string)obj["value"], options);
            selector.Show();
        }

        private void AddTextInput(string key, int index, Action<object> onChange)
        {
            var textInput = AddTextInput(_textInputTemplate.Copy(key), index);
            textInput.OnChange += value => onChange(value);
            textInput.Element.name = key;
            textInput.Show();
        }

        private void AddComboInput(string key, int index, Action<object> onChange)
        {
            var comboInput = AddComboInput(_comboInputTemplate.Copy(key), index);
            comboInput.OnChange += value => onChange(value);
            comboInput.Element.name = key;
            comboInput.Show();
        }

        private void AddNumberInput(string key, int index, Action<object> onChange)
        {
            var numberInput = AddNumberInput(_numberInputTemplate.Copy(key), index);
            numberInput.OnChange += value => onChange(value);
            numberInput.Element.name = key;
            numberInput.Show();
        }

    }
}
