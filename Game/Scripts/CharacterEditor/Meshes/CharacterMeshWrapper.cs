using CharacterEditor.Mesh;
using CharacterEditor.Services;
using UnityEngine;

namespace CharacterEditor
{
    public class CharacterMeshWrapper
    {
        private readonly IMeshInstanceCreator _meshFactory;
        private readonly Transform _meshBone;
        public CharacterMesh Mesh { get; private set; }

        public GameObject MeshInstance { get; private set; }

        public bool IsEmptyMesh =>
            Mesh.LoadedMeshObject == null;

        public CharacterMeshWrapper(IMeshInstanceCreator meshFactory, IMeshLoader meshLoader, Transform meshBone, IDataManager dataManager, MeshType meshType, string characterRace)
        {
            _meshFactory = meshFactory;
            _meshBone = meshBone;

            Mesh = MeshFactory.Create(meshLoader, dataManager, meshType, characterRace);
        }

        public GameObject CreateMeshInstance()
        {
            if (MeshInstance == null)
                MeshInstance = _meshFactory.CreateMeshInstance(Mesh, _meshBone);
            return MeshInstance;
        }


        public void ClearPrevMesh()
        {
            if (MeshInstance == null) return;

            MeshInstance.SetActive(false);
            Object.Destroy(MeshInstance);
            MeshInstance = null;

            Mesh.ClearPrevMesh();
        }
    }
}