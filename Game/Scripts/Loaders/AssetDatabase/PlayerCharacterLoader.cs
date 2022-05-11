#if UNITY_EDITOR

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class PlayerCharacterLoader : DataLoader<PlayableNpcConfig>
        {
            protected override string ConfigsPath =>
                $"{AssetsConstants.GameStaticDataPath}/PlayerCharacters";
        }
    }
}
#endif