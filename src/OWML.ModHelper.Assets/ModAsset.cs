﻿using System;
using OWML.Common;
using UnityEngine;

namespace OWML.ModHelper.Assets
{
	public class ModAsset<T> : MonoBehaviour, IModAsset<T>
	{
		public event Action<T> Loaded;

		[Obsolete("Use Loaded instead.")]
		public Action<T> OnLoaded { get; set; }

		public T Asset { get; private set; }

		public void SetAsset(T asset)
		{
			Asset = asset;
			OnLoaded?.Invoke(asset);
			Loaded?.Invoke(asset);
		}

		public void Start() =>
			DontDestroyOnLoad(gameObject);

		public T1 AddComponent<T1>() where T1 : Component =>
			gameObject.AddComponent<T1>();
	}
}
