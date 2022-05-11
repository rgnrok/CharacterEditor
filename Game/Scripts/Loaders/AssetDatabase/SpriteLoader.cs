﻿#if UNITY_EDITOR
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
            private static readonly string ItemIconAtlasPath =
                $"{AssetsConstants.CharacterEditorRootPath}/Sprites/ItemIcons";

            private static readonly string PortraitIconAtlasPath =
                $"{AssetsConstants.CharacterEditorRootPath}/Sprites/Portraits";

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

            private void LoadIcon(string atlasPath, string iconName, Action<Sprite> callback)
            {
                if (_atlasCache.TryGetValue(atlasPath, out var atlas))
                {
                    callback.Invoke(atlas.GetSprite(iconName));
                    return;
                }

                var paths = AssetDatabase.FindAssets("t:SpriteAtlas", new[] {atlasPath});
                if (paths.Length == 0)
                {
                    callback.Invoke(null);
                    return;
                }

                var loadedAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(paths[0]));
                _atlasCache[atlasPath] = loadedAtlas;
                callback.Invoke(loadedAtlas.GetSprite(iconName));
            }

            private Sprite LoadIcon(string atlasPath, string iconName)
            {
                if (_atlasCache.TryGetValue(atlasPath, out var atlas))
                    return atlas.GetSprite(iconName);

                var paths = AssetDatabase.FindAssets("t:SpriteAtlas", new[] {atlasPath});
                if (paths.Length == 0) return null;

                var loadedAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(paths[0]));
                _atlasCache[atlasPath] = loadedAtlas;
                return loadedAtlas.GetSprite(iconName);
            }
        }
    }
}
#endif