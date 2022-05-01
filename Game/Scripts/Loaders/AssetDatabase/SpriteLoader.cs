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
            private static readonly string ITEM_ICON_ATLAS_PATH =
                $"{AssetsConstants.CharacterEditorRootPath}/Sprites/ItemIcons";

            private static readonly string PORTRAIT_ICON_ATLAS_PATH =
                $"{AssetsConstants.CharacterEditorRootPath}/Sprites/Portraits";

            protected Dictionary<string, SpriteAtlas> atlasCache = new Dictionary<string, SpriteAtlas>();

            public void LoadItemIcon(string iconName, Action<Sprite> callback)
            {
                LoadIcon(ITEM_ICON_ATLAS_PATH, iconName, callback);
            }

            public void LoadPortrait(string portraitName, Action<Sprite> callback)
            {
                LoadIcon(PORTRAIT_ICON_ATLAS_PATH, portraitName, callback);
            }

            public async Task<Sprite> LoadItemIcon(string iconName)
            {
                return LoadIcon(ITEM_ICON_ATLAS_PATH, iconName);
            }

            public async Task<Sprite> LoadPortrait(string portraitName)
            {
                return LoadIcon(PORTRAIT_ICON_ATLAS_PATH, portraitName);
            }

            private void LoadIcon(string atlasPath, string iconName, Action<Sprite> callback)
            {
                if (atlasCache.TryGetValue(atlasPath, out var atlas))
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
                atlasCache[atlasPath] = loadedAtlas;
                callback.Invoke(loadedAtlas.GetSprite(iconName));
            }

            private Sprite LoadIcon(string atlasPath, string iconName)
            {
                if (atlasCache.TryGetValue(atlasPath, out var atlas))
                    return atlas.GetSprite(iconName);

                var paths = AssetDatabase.FindAssets("t:SpriteAtlas", new[] {atlasPath});
                if (paths.Length == 0) return null;

                var loadedAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(paths[0]));
                atlasCache[atlasPath] = loadedAtlas;
                return loadedAtlas.GetSprite(iconName);
            }
        }
    }
}
#endif