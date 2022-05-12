namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class DataPathProvider : IDataPathProvider
        {
            public string GetPath(PathData pathData)
            {
                return pathData.bundlePath;
            }
        }
    }
}