namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class PathDataProvider : IPathDataProvider
        {
            public string GetPath(PathData pathData)
            {
                return pathData.bundlePath;
            }
        }
    }
}