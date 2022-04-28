using System;
using Game;
using UnityEngine;
using UnityEngine.U2D;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class Sprite : CommonLoader<SpriteAtlas>, IIconLoader
        {
            protected const string ITEM_ICON_ATLAS_BUNDLE_NAME = "item_icons/ItemIcons";
            protected const string PORTRAIT_ICON_ATLAS_BUNDLE_NAME = "portraits/Portraits";

            public Sprite(ICoroutineRunner coroutineRunner) : base(coroutineRunner)
            {
            }

            public void LoadItemIcon(string iconName, Action<UnityEngine.Sprite> callback)
            {
                LoadIcon(ITEM_ICON_ATLAS_BUNDLE_NAME, iconName, callback);
            }

            public void LoadPortrait(string portraitName, Action<UnityEngine.Sprite> callback)
            {
                LoadIcon(PORTRAIT_ICON_ATLAS_BUNDLE_NAME, portraitName, callback);
            }
            
            private void LoadIcon(string atlasPath, string iconName, Action<UnityEngine.Sprite> callback)
            {
                LoadByPath(atlasPath, (path, loadedAtlas) =>
                {
                    callback(loadedAtlas.GetSprite(iconName));
                });
            }
        }
    }
}