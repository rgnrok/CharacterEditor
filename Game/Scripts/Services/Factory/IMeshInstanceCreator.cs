using CharacterEditor.Mesh;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface IMeshInstanceCreator : IService
    {
        GameObject CreateMeshInstance(CharacterMesh characterMesh, Transform anchor);
    }
}