#if UNITY_EDITOR
namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class DataPathProvider : IDataPathProvider
        {
            public string GetPath(PathData pathData)
            {
                return pathData.path;
            }
        }
    }
}
#endif