namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class PathDataProvider : IPathDataProvider
        {
            public string GetPath(PathData pathData)
            {
                return pathData.addressPath;
            }
        }
    }
}