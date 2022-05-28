using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class CharacterGameObjectData : EntityGameObjectData<CharacterConfig>
{
    public GameObject CharacterObject => Entity;
    public GameObject PreviewCharacterObject { get; private set; }


    public Dictionary<MeshType, Transform> meshBones;
    public Dictionary<MeshType, Transform> previewMeshBones;

    public Renderer[] ShortRobeMeshes { get; private set; }
    public Renderer[] LongRobeMeshes { get; private set; }
    public Renderer[] CloakMeshes { get; private set; }

    public Renderer[] PreviewSkinMeshes { get; private set; }
    public Renderer[] PreviewShortRobeMeshes { get; private set; }
    public Renderer[] PreviewLongRobeMeshes { get; private set; }
    public Renderer[] PreviewCloakMeshes { get; private set; }

    private readonly Transform _head;

    public CharacterGameObjectData(CharacterConfig config, GameObject character): base(config, character)
    {
        _head = character.transform.FindTransform(config.headBone);

        var entityTransform = Entity.transform;
        ShortRobeMeshes = ParseSkinMeshes(entityTransform, Config.shortRobeMeshes);
        LongRobeMeshes = ParseSkinMeshes(entityTransform, Config.longRobeMeshes);
        CloakMeshes = ParseSkinMeshes(entityTransform, Config.cloakMeshes);

        ParseMeshBones(entityTransform, out meshBones);
    }

    public CharacterGameObjectData(CharacterConfig config, GameObject character, GameObject characterPreview): this(config, character)
    {
        InitPreviewPrefab(characterPreview);
    }

    public Transform GetHead()
    {
        return _head;
    }

    public void InitPreviewPrefab(GameObject characterPreview)
    {
        PreviewCharacterObject = characterPreview;

        if (PreviewCharacterObject == null) return;

        var previewTransform = PreviewCharacterObject.transform;
        PreviewSkinMeshes = ParseSkinMeshes(previewTransform, Config.skinnedMeshes);
        PreviewShortRobeMeshes = ParseSkinMeshes(previewTransform, Config.shortRobeMeshes);
        PreviewLongRobeMeshes = ParseSkinMeshes(previewTransform, Config.longRobeMeshes);
        PreviewCloakMeshes = ParseSkinMeshes(previewTransform, Config.cloakMeshes);

        ParseMeshBones(previewTransform, out previewMeshBones);
    }

    private void ParseMeshBones(Transform transform, out Dictionary<MeshType, Transform> bones)
    {
        bones = new Dictionary<MeshType, Transform>();
        foreach (var mesh in Config.availableMeshes)
            bones[mesh.mesh] = transform.FindTransform(mesh.boneName);
    }
}
