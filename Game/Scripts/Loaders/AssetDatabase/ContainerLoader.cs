#if UNITY_EDITOR

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class ContainerLoader : DataLoader<ContainerConfig>
        {
            protected override string ConfigsPath =>
                $"{AssetsConstants.CharacterStaticDataPath}/Containers";
        }
    }
}
#endif