using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class EntityGameObjectData<TConfig> where TConfig: EntityConfig
{
    public TConfig Config { get; private set; }
    public Renderer[] SkinMeshes { get; private set; }
    public GameObject Entity { get; private set; }

    public Animator Animator { get; private set; }

    public EntityGameObjectData(TConfig config, GameObject entityObject)
    {
        Config = config;
        Entity = entityObject;
        Animator = Entity.GetComponent<Animator>();

        SkinMeshes = ParseSkinMeshes(Entity.transform, Config.skinnedMeshes);
    }

    protected Renderer[] ParseSkinMeshes(Transform transform, string[] skinnedMeshes)
    {
        var skinList = new List<Renderer>();

        foreach (var skinMesh in skinnedMeshes)
        {
            var obj = transform.FindTransform(skinMesh);
            if (obj == null) continue;

            skinList.Add(obj.GetComponent<Renderer>());
        }

        return skinList.ToArray();
    }
}
