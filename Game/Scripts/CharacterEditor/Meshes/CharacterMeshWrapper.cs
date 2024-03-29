﻿using CharacterEditor.Mesh;
using CharacterEditor.Services;
using UnityEngine;

namespace CharacterEditor
{
    public class CharacterMeshWrapper
    {
        private readonly IMeshInstanceCreator _meshFactory;
        private readonly Transform _meshBone;
        public CharacterMesh Mesh { get; }

        public GameObject MeshInstance { get; private set; }

        private MeshRenderer[] _meshRenders;
        public MeshRenderer[] MeshRenders
        {
            get
            {
                if (_meshRenders == null && MeshInstance != null) _meshRenders = MeshInstance.GetComponentsInChildren<MeshRenderer>();
                return _meshRenders;
            }
        }

        public bool IsEmptyMesh =>
            Mesh.LoadedMeshObject == null;

        public CharacterMeshWrapper(IMeshInstanceCreator meshFactory, Transform meshBone, CharacterMesh mesh)
        {
            _meshFactory = meshFactory;
            _meshBone = meshBone;
            Mesh = mesh;
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
            _meshRenders = null;

            Mesh.ClearPrevMesh();
        }
    }
}