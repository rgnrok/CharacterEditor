using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    public interface ISpriteLoader : IService
    {
        void LoadItemIcon(string iconName, Action<Sprite> callback);
        void LoadPortrait(string portraitName, Action<Sprite> callback);

        Task<Sprite> LoadItemIcon(string iconName);
        Task<Sprite> LoadPortrait(string portraitName);
    }
}
