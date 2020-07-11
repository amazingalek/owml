﻿using System;
using UnityEngine;

namespace OWML.Common.Menus
{
    public interface IModInput<T> : IModInputBase
    {
        event Action<T> OnChange;
        T Value { get; set; }
    }
}