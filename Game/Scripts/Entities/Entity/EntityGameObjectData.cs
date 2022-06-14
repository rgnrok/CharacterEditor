using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class EntityGameObjectData<TConfig> where TConfig: EntityConfig
{
    public TConfig Config { get; }
    public Renderer[] SkinMeshes { get; }
    public GameObject Entity { get; }

    public Animator Animator { get; }

    public EntityGameObjectData(TConfig config, GameObject entityObject)
    {
        Config = config;
        Entity = entityObject;
        Animator = Entity.GetComponentInChildren<Animator>();

        SkinMeshes = ParseSkinMeshes(Entity.transform, Config.skinnedMeshes);
    }

    protected Renderer[] ParseSkinMeshes(Transform transform, IEnumerable<string> skinnedMeshes)
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
