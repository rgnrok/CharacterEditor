using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    [Serializable]
    public class EquipItemSlotItem
    {
        public EquipItemSlot itemSlot;
        public ItemData item;

        public EquipItemSlotItem(EquipItemSlot slot, ItemData itemData)
        {
            itemSlot = slot;
            item = itemData;
        }
    }

    [Serializable]
    public class MeshTypeFaceMeshPath
    {
        public MeshType meshType;
        public PathData meshPath;

        public MeshTypeFaceMeshPath(MeshType type, string path)
        {
            meshType = type;
            meshPath = new PathData(path);
        }
    }

    [Serializable]
    public class PlayableNpcConfig : ScriptableObject, IData
    {
        public string guid;
        public string Guid { get { return guid; } }


        public CharacterConfig characterConfig;

        public PathData texturePath;
        public PathData faceMeshTexturePath;

        public string portraitIconPath;
        public string portraitIconName;

        public EquipItemSlotItem[] equipItems;
        public MeshTypeFaceMeshPath[] faceMeshs;

    }
}
