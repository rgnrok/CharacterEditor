#if UNITY_EDITOR

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class EnemyLoader : DataLoader<EnemyConfig>
        {
            protected override string ConfigsPath =>
                $"{AssetsConstants.CharacterStaticDataPath}/Enemies";
        }
    }
}
#endif