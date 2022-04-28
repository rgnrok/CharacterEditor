#if UNITY_EDITOR
using CharacterEditor.CharacterInventory;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class ItemLoader : DataLoader<ItemData>
        {
            protected override string ConfigsPath =>
                $"{AssetsConstants.GameStaticDataPath}/Items";
        }
    }
}
#endif