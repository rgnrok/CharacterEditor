using System;

namespace CharacterEditor
{
    [Serializable]
    public class PathData
    {
        public string path;
        public string bundlePath;
        public string addressPath;

        public PathData(string mPath)
        {
            path = mPath;
            bundlePath = null;
            addressPath = null;
        }
    }
}