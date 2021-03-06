﻿using UnityEngine;

namespace OWML.Common
{
	public interface IModInputTextures
	{
		Texture2D KeyTexture(string key);

		Texture2D KeyTexture(KeyCode key);

		void FillTextureLibrary();
	}
}
