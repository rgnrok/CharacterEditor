using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using StatSystem;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        [Serializable]
        public class CharacterItemData
        {
            public string guid;
            public string dataGuid;
            public StatData[] stats;

            public CharacterItemData(Item item)
            {
                var eqItem = item as EquipItem;
                if (eqItem != null) SetUpItemData(eqItem);
                else SetUpItemData(item);
            }

            public void SetUpItemData(Item item)
            {
                guid = item.Guid;
                dataGuid = item.Guid;
            }


            public void SetUpItemData(EquipItem item)
            {
                guid = item.Guid;
                dataGuid = item.DataGuid;

                var statList = new List<StatData>();
                foreach (var statPair in item.Stats)
                {
                    var statType = statPair.Key;
                    foreach (var modifier in statPair.Value)
                    {
                        var statData = new StatData();
                        statData.type = statType;
                        statData.modType = modifier.Type;
                        statData.value = modifier.Value;

                        statList.Add(statData);
                    }
                }

                stats = statList.ToArray();
            }
        }

        [Serializable]
        public class CharacterSaveData : EntitySaveData
        {
            public bool isMainCharacter;
            public Dictionary<EquipItemSlot, CharacterItemData> equipItems;
            public Dictionary<MeshType, string> faceMeshItems;
            public string portrait;

            public List<string> inventoryItems; 
            public Dictionary<int, CharacterItemData> inventoryCells;


            public CharacterSaveData()
            {
                equipItems = new Dictionary<EquipItemSlot, CharacterItemData>();
                faceMeshItems = new Dictionary<MeshType, string>();
                inventoryCells = new Dictionary<int, CharacterItemData>();
                inventoryItems = new List<string>();
            }

            public CharacterSaveData(string mGuid, string mConfigGuid, string mPortrait) : this()
            {
                UpdateStats(new DefaultStatCollection());

                guid = mGuid;
                configGuid = mConfigGuid;
                position = new SerializableVector3();
                rotation = new SerializableQuaternion();
                portrait = mPortrait;
            }

            public CharacterSaveData(Character character, Dictionary<int, CharacterItemData> cells = null, List<string> items = null) : this()
            {
                if (cells != null) inventoryCells = cells;
                if (items != null) inventoryItems = items;

                SetUpItems(character.EquipItems, character.FaceMeshItems);
                UpdateStats(character.StatCollection);

                guid = character.Guid;
                configGuid = character.ConfigGuid;
                position = character.GameObjectData.CharacterObject.transform.position;
                rotation = character.GameObjectData.CharacterObject.transform.rotation;
                portrait = character.Portrait.name.Replace("(Clone)", "").Trim();
            }

            public CharacterSaveData(SerializationInfo info, StreamingContext context): base(info, context)
            {
                equipItems = (Dictionary<EquipItemSlot, CharacterItemData>)info.GetValue("equipItems", typeof(Dictionary<EquipItemSlot, CharacterItemData>));
                faceMeshItems = (Dictionary<MeshType, string>)info.GetValue("faceMeshItems", typeof(Dictionary<MeshType, string>));
                inventoryItems = (List<string>)info.GetValue("inventoryItems", typeof(List<string>));
                inventoryCells = (Dictionary<int, CharacterItemData>)info.GetValue("inventoryCeils", typeof(Dictionary<int, CharacterItemData>));
                portrait = info.GetString("portrait");
            }

            protected void SetUpItems(Dictionary<EquipItemSlot, EquipItem> characterEquipItems, Dictionary<MeshType, FaceMesh> characterFaceMeshItems)
            {
                equipItems = new Dictionary<EquipItemSlot, CharacterItemData>();
                foreach (var equipPair in characterEquipItems)
                {
                    var itemData = new CharacterItemData(equipPair.Value);
                    equipItems[equipPair.Key] = itemData;
                }

                faceMeshItems = new Dictionary<MeshType, string>();
                foreach (var faceMesh in characterFaceMeshItems)
                    faceMeshItems[faceMesh.Key] = faceMesh.Value.MeshPath;
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("equipItems", equipItems);
                info.AddValue("faceMeshItems", faceMeshItems);
                info.AddValue("inventoryItems", inventoryItems);
                info.AddValue("inventoryCeils", inventoryCells);
                info.AddValue("portrait", portrait);
            }
        }
    }
}