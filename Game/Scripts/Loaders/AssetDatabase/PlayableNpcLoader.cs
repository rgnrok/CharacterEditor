#if UNITY_EDITOR

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class PlayableNpcLoader : DataLoader<PlayableNpcConfig>
        {
            protected override string ConfigsPath =>
                $"{AssetsConstants.GameStaticDataPath}/PlayableNpc";
        }
    }
}
#endif