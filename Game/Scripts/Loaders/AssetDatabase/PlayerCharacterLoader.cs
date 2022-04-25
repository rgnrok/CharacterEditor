#if UNITY_EDITOR

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {

        public class PlayerCharacterLoader : DataLoader<PlayerCharacterConfig>
        {
            protected override string ConfigsPath =>
                $"{AssetsConstants.CharacterStaticDataPath}/PlayerCharacters";
        }
    }
}
#endif