using UnityEngine;

namespace CharacterEditor.StaticData
{
    [CreateAssetMenu(fileName = "LoaderData", menuName = "StaticData/Loader")]

    public class LoaderStaticData : ScriptableObject
    {
        public LoaderType LoaderType;

        public MeshAtlasType MeshAtlasType;

    }
}