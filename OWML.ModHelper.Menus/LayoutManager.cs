﻿using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using OWML.Common.Menus;
using System.Collections.Generic;
using OWML.ModHelper.Events;

namespace OWML.ModHelper.Menus
{
    public class LayoutManager : ILayoutManager
    {
        private const int FontSize = 36;
        private static readonly Vector2 NormalPivot = new Vector2(0.5f, 0.5f);

        public LayoutGroup LayoutGroup { get; private set; }
        public int ChildCount => LayoutGroup.transform.childCount;

        private readonly UIStyleManager _styleManager;
        private readonly ModUIStyleApplier _styleApplier;
        private readonly Vector3 _scale;
        private readonly HashSet<Graphic> _constantGraphics = new HashSet<Graphic>();
        private readonly HashSet<Graphic> _backingGraphics = new HashSet<Graphic>();

        public LayoutManager(LayoutGroup layout, UIStyleManager styleManager, ModUIStyleApplier styleApplier, Vector3 scale, Graphic[] constantGraphics, Graphic[] backGraphics) : this(layout, styleManager, styleApplier, scale, constantGraphics)
        {
            styleApplier.SetBackround(backGraphics);
        }

        public LayoutManager(LayoutGroup layout, UIStyleManager styleManager, ModUIStyleApplier styleApplier, Vector3 scale, Graphic[] constantGraphics) : this(layout, styleManager, styleApplier, scale)
        {
            Array.ForEach(constantGraphics, element => _constantGraphics.Add(element));
        }

        public LayoutManager(LayoutGroup layout, UIStyleManager styleManager, ModUIStyleApplier styleApplier, Vector3 scale)
        {
            _scale = scale;
            _styleManager = styleManager;
            _styleApplier = styleApplier;
            LayoutGroup = layout;
            LayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            LayoutGroup.transform.localPosition = Vector3.zero;
            ((RectTransform)LayoutGroup.transform).pivot = NormalPivot;
            UpdateState();
        }

        public void UpdateState()
        {
            var currentTexts = LayoutGroup.gameObject.GetComponentsInChildren<Text>();
            _styleApplier.SetTexts(currentTexts);
            Graphic[] currentGraphics = currentTexts;
            Array.Resize<Graphic>(ref currentGraphics, currentTexts.Length + _constantGraphics.Count);
            int i = currentTexts.Length;
            foreach (var graphic in _constantGraphics)
            {
                currentGraphics[i] = graphic;
                i++;
            }
            _styleApplier.SetForeground(currentGraphics);
        }

        public void Clear()
        {
            var childCount = LayoutGroup.transform.childCount;
            for (var i = childCount - 1; i >= 0; i--)
            {
                if (!(_constantGraphics.Contains(LayoutGroup.transform.GetChild(i).gameObject.GetComponent<Graphic>())
                    || _backingGraphics.Contains(LayoutGroup.transform.GetChild(i).gameObject.GetComponent<Graphic>())))
                {
                    LayoutGroup.transform.GetChild(i).gameObject.SetActive(false);
                    GameObject.Destroy(LayoutGroup.transform.GetChild(i).gameObject);
                }
            }
        }

        public void AddText(string text)
        {
            AddTextAt(text, LayoutGroup.transform.childCount);
        }

        public void AddTextAt(string text, int index)
        {
            var textObject = new GameObject("Text", new Type[] { typeof(RectTransform) });
            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = FontSize;
            textComponent.font = _styleManager.GetMenuFont();
            textComponent.color = _styleManager.GetButtonForegroundMenuColor(UIElementState.NORMAL);
            textComponent.alignment = TextAnchor.MiddleCenter;
            textObject.AddComponent<LayoutElement>();
            textObject.transform.SetParent(LayoutGroup.transform);
            textObject.transform.localScale = _scale;
            ((RectTransform)textObject.transform).sizeDelta = new Vector2(textComponent.preferredWidth, ((RectTransform)textObject.transform).sizeDelta.y);
            ((RectTransform)textObject.transform).pivot = NormalPivot;
            textObject.transform.SetSiblingIndex(index);
        }

        public void AddPicture(Texture2D texture, float scale = 1.0f)
        {
            AddPictureAt(texture, LayoutGroup.transform.childCount, scale);
        }

        public void AddPictureAt(Texture2D texture, int index, float scale = 1.0f)
        {
            var keySprite = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), NormalPivot);
            var keyObject = new GameObject("ButtonImage", new Type[] { typeof(RectTransform) });
            var keyPicture = keyObject.AddComponent<Image>();
            keyPicture.sprite = keySprite;
            keyPicture.SetLayoutDirty();
            keyObject.AddComponent<LayoutElement>();
            keyObject.transform.SetParent(LayoutGroup.transform);
            keyObject.transform.localScale = _scale;
            ((RectTransform)keyObject.transform).sizeDelta =
                new Vector2((float)texture.width * scale, (float)texture.height * scale);
            ((RectTransform)keyObject.transform).pivot = NormalPivot;
            keyObject.transform.SetSiblingIndex(index);
        }
    }
}
