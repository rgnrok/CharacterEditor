using UnityEngine;

namespace CharacterEditor.StaticData
{
    [CreateAssetMenu(fileName = "GameData", menuName = "StaticData/Loader")]

    public class GameStaticData : ScriptableObject
    {
        public LoaderType LoaderType;

        public MeshAtlasType MeshAtlasType;

        public Material ArmorMergeMaterial;
        public Material ClothMergeMaterial;
        public Material ModelMaterial;
        public Material PreviewMaterial;

    }
}