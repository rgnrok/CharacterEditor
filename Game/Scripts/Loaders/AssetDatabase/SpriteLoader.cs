#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class SpriteLoader : CommonLoader<SpriteAtlas>, ISpriteLoader
        {
            public static readonly string ItemIconAtlasPath =
                $"{AssetsConstants.CharacterEditorRootPath}/Sprites/ItemIcons.spriteatlas";

            public static readonly string PortraitIconAtlasPath =
                $"{AssetsConstants.CharacterEditorRootPath}/Sprites/Portraits.spriteatlas";

            private readonly Dictionary<string, SpriteAtlas> _atlasCache = new Dictionary<string, SpriteAtlas>();

            public void LoadItemIcon(string iconName, Action<Sprite> callback)
            {
                LoadIcon(ItemIconAtlasPath, iconName, callback);
            }

            public void LoadPortrait(string portraitName, Action<Sprite> callback)
            {
                LoadIcon(PortraitIconAtlasPath, portraitName, callback);
            }

            public Task<Sprite> LoadItemIcon(string iconName)
            {
                return Task.FromResult(LoadIcon(ItemIconAtlasPath, iconName));
            }

            public Task<Sprite> LoadPortrait(string portraitName)
            {
                return Task.FromResult(LoadIcon(PortraitIconAtlasPath, portraitName));
            }

            public Task<SpriteAtlas> LoadPortraits()
            {
                return Task.FromResult(LoadAtlas(PortraitIconAtlasPath));
            }

            private void LoadIcon(string atlasPath, string iconName, Action<Sprite> callback)
            {
                var loadedAtlas = LoadAtlas(atlasPath);
                callback.Invoke(loadedAtlas?.GetSprite(iconName));
            }

            private Sprite LoadIcon(string atlasPath, string iconName)
            {
                var loadedAtlas = LoadAtlas(atlasPath);
                return loadedAtlas?.GetSprite(iconName);
            }

            private SpriteAtlas LoadAtlas(string atlasPath)
            {
                if (_atlasCache.TryGetValue(atlasPath, out var atlas))
                    return atlas;

                var loadedAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                _atlasCache[atlasPath] = loadedAtlas;
                return loadedAtlas;
            }
        }
    }
}
#endif