namespace CharacterEditor.Services
{
    public interface IStaticDataService : IService
    {
        LoaderType LoaderType { get; }

        MeshAtlasType MeshAtlasType { get; }
    }
}