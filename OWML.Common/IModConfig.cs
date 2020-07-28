﻿using System;
using System.Collections.Generic;

namespace OWML.Common
{
    public interface IModConfig
    {
        bool Enabled { get; set; }
        [Obsolete("Use ModManifest.RequireVR instead")]
        bool RequireVR { get; set; }
        Dictionary<string, object> Settings { get; set; }

        T GetSettingsValue<T>(string key);
        void SetSettingsValue(string key, object value);

        IModConfig Copy();
    }
}
