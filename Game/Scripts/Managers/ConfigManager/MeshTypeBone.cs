using System;

namespace CharacterEditor
{
    [Serializable]
    public class MeshTypeBone
    {
        public MeshType mesh;
        public string boneName;

        public MeshTypeBone(MeshType type, string bone)
        {
            mesh = type;
            boneName = bone;
        }
    }
}
