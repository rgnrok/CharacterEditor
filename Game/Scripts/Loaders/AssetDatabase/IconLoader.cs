#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class IconLoader : CommonLoader<SpriteAtlas>, IIconLoader
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
        }
    }
}
#endif