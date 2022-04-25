using System;
using UnityEngine;

namespace CharacterEditor
{
    public interface IIconLoader : IService
    {
        void LoadItemIcon(string iconName, Action<Sprite> callback);
        void LoadPortrait(string portraitName, Action<Sprite> callback);
    }
}
