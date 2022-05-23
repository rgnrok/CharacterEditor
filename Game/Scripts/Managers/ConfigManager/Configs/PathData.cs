using System;

namespace CharacterEditor
{
    [Serializable]
    public struct PathData
    {
        public string path;
        public string bundlePath;
        public string addressPath;

        public PathData(string mPath) : this()
        {
            path = mPath;
        }
    }
}