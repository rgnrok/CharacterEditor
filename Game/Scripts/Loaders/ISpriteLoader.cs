using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace CharacterEditor
{
    public interface ISpriteLoader : ICleanable
    {
        void LoadItemIcon(string iconName, Action<Sprite> callback);
        void LoadPortrait(string portraitName, Action<Sprite> callback);

        Task<Sprite> LoadItemIcon(string iconName);
        Task<Sprite> LoadPortrait(string portraitName);
        Task<SpriteAtlas> LoadPortraits();
    }
}
