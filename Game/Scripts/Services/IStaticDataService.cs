using CharacterEditor.StaticData;

namespace CharacterEditor.Services
{
    public interface IStaticDataService : IService
    {
        LoaderType LoaderType { get; }

        MeshAtlasType MeshAtlasType { get; }
        LevelStaticData ForLevel(string sceneKey);
    }
}