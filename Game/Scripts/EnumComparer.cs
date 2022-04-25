using System.Collections.Generic;

namespace CharacterEditor
{
    public static class EnumComparer
    {
        public class TextureTypeComparer : IEqualityComparer<TextureType> { public bool Equals(TextureType x, TextureType y) { return x == y; } public int GetHashCode(TextureType x) { return (int)x; } }
        public static readonly TextureTypeComparer TextureType = new TextureTypeComparer();

        public class MeshTypeComparer : IEqualityComparer<MeshType> { public bool Equals(MeshType x, MeshType y) { return x == y; } public int GetHashCode(MeshType x) { return (int)x; } }
        public static readonly MeshTypeComparer MeshType = new MeshTypeComparer();

    }
}