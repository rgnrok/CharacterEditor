using System;

namespace CharacterEditor
{
    [Serializable]
    public class PathData
    {
        public string path;
        public string bundlePath;
        public string addressPath;

        public PathData()
        {
        }

        public PathData(string mPath) : this()
        {
            path = mPath;
        }
    }
}