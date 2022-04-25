using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class EntityGameObjectData<TConfig> where TConfig: EntityConfig
{
    public TConfig Config { get; private set; }
    public SkinnedMeshRenderer[] SkinMeshes { get; private set; }
    public GameObject Entity { get; private set; }

    public Animator Animator { get; private set; }

    public EntityGameObjectData(TConfig config, GameObject entityObject)
    {
        Config = config;
        Entity = entityObject;
        Animator = Entity.GetComponent<Animator>();

        SkinMeshes = ParseSkinMeshes(Entity.transform, Config.skinnedMeshes);
    }

    protected SkinnedMeshRenderer[] ParseSkinMeshes(Transform transform, string[] skinnedMeshes)
    {
        var skinList = new List<SkinnedMeshRenderer>();

        foreach (var skinMesh in skinnedMeshes)
        {
            var obj = transform.FindTransform(skinMesh);
            if (obj == null) continue;

            skinList.Add(obj.GetComponent<SkinnedMeshRenderer>());
        }

        return skinList.ToArray();
    }
}
