using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class SpriteLoader : CommonLoader<SpriteAtlas>, ISpriteLoader
        {
            public const string ITEM_ICON_ATLAS_BUNDLE_NAME = "item_icons/ItemIcons";
            public const string PORTRAIT_ICON_ATLAS_BUNDLE_NAME = "portraits/Portraits";

            public void LoadItemIcon(string iconName, Action<Sprite> callback)
            {
                LoadIcon(ITEM_ICON_ATLAS_BUNDLE_NAME, iconName, callback);
            }

            public void LoadPortrait(string portraitName, Action<Sprite> callback)
            {
                LoadIcon(PORTRAIT_ICON_ATLAS_BUNDLE_NAME, portraitName, callback);
            }

            public async Task<Sprite> LoadItemIcon(string iconName)
            {
                return await LoadIcon(ITEM_ICON_ATLAS_BUNDLE_NAME, iconName);
            }

            public async Task<Sprite> LoadPortrait(string portraitName)
            {
                return await LoadIcon(PORTRAIT_ICON_ATLAS_BUNDLE_NAME, portraitName);
            }

            public Task<SpriteAtlas> LoadPortraits()
            {
                return LoadByPath(PORTRAIT_ICON_ATLAS_BUNDLE_NAME);
            }

            private async Task<Sprite> LoadIcon(string atlasPath, string iconName)
            {
                var loadedAtlas = await LoadByPath(atlasPath);
                return loadedAtlas.GetSprite(iconName);
            }

            private void LoadIcon(string atlasPath, string iconName, Action<Sprite> callback)
            {
                LoadByPath(atlasPath, (path, loadedAtlas) =>
                {
                    callback(loadedAtlas.GetSprite(iconName));
                });
            }
        }
    }
}