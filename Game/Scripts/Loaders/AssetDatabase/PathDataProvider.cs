#if UNITY_EDITOR
namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class PathDataProvider : IPathDataProvider
        {
            public string GetPath(PathData pathData)
            {
                return pathData.path;
            }
        }
    }
}
#endif