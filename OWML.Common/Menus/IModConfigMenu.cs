﻿namespace OWML.Common.Menus
{
    public interface IModConfigMenu : IBaseConfigMenu
    {
        IModBehaviour Mod { get; }
    }
}
