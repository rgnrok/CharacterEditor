using Assets.Game.Scripts.Loaders;
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

        private GameObject _meshInstance;
        public GameObject MeshInstance
        {
            get
            {
                if (_meshInstance == null)
                    _meshInstance = _meshFactory.CreateMeshInstance(Mesh, _meshBone);
                return _meshInstance;

            }
        }
        
        public bool IsEmptyMesh =>
            Mesh.LoadedMeshObject == null;

        public CharacterMeshWrapper(IMeshInstanceCreator meshFactory, IMeshLoader meshLoader, Transform meshBone, IDataManager dataManager, MeshType meshType, string characterRace)
        {
            _meshFactory = meshFactory;
            _meshBone = meshBone;

            Mesh = MeshFactory.Create(meshLoader, dataManager, meshType, characterRace);
        }

     

        public void ClearPrevMesh()
        {
            if (_meshInstance != null)
            {
                _meshInstance.SetActive(false);
                Object.Destroy(_meshInstance);
                _meshInstance = null;
            }

            Mesh.ClearPrevMesh();
        }
    }
}