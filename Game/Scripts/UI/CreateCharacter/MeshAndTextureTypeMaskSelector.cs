using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public class MeshAndTextureTypeMaskSelector : MonoBehaviour
    {
        [EnumFlag]
        public MeshType meshTypeMask;
        protected MeshType[] meshTypes;

        [EnumFlag]
        public TextureType textureTypeMask;
        protected TextureType[] textureTypes;

        public void Start() {
            List<MeshType> meshList = new List<MeshType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(MeshType))) {
                int checkBit = (int)meshTypeMask & (int)enumValue;
                if (checkBit != 0)
                    meshList.Add((MeshType)enumValue);
            }
            meshTypes = meshList.ToArray();

            List<TextureType> textureList = new List<TextureType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(TextureType))) {
                int checkBit = (int)textureTypeMask & (int)enumValue;
                if (checkBit != 0)
                    textureList.Add((TextureType)enumValue);
            }
            textureTypes = textureList.ToArray();
        }
    }
}