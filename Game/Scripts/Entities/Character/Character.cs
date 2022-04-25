using System;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using StatSystem;
using UnityEngine;
using Attribute = StatSystem.Attribute;

namespace CharacterEditor
{
    public class Character: Entity<CharacterGameObjectData, CharacterConfig>, IBattleEntity, IAttacked
    {
        public readonly Dictionary<EquipItemSlot, EquipItem> EquipItems = new Dictionary<EquipItemSlot, EquipItem>();
        public readonly Dictionary<MeshType, FaceMesh> FaceMeshItems = new Dictionary<MeshType, FaceMesh>();
        public readonly Dictionary<int, CharacterItemData> inventoryCeils = new Dictionary<int, CharacterItemData>();

        public Texture2D FaceMeshTexture { get; private set; }
        public Sprite Portrait { get; private set; }
        public CharacterFSM FSM { get; private set; }

        public FSM BaseFSM { get; }
        public GameObject EntityGameObject { get { return GameObjectData.Entity; } }
        public CharacterAttackManager AttackManager { get; private set; }
        public PlayerMoveComponent MoveComponent { get; private set; }

        public Vital ActionPoints { get { return StatCollection.GetStat<Vital>(StatType.ActionPoint); } }
        public float Speed { get { return StatCollection.GetStat<Attribute>(StatType.Speed).StatValue / 100f; } }

        public event Action<EquipItem, EquipItemSlot> OnEquipItem;
        public event Action<EquipItem> OnUnequipItem;

        public event Action<string, Item> OnAddToInventory;
        public event Action<string, Item> OnRemoveFromInventory;

        public event Action<IBattleEntity> OnDied;


        public static Character Create(CharacterSaveData data, CharacterGameObjectData gameObjectData, Texture2D texture, Texture2D faceMeshTexture, Sprite portrait)
        {
            var character = new Character(data, gameObjectData, texture, faceMeshTexture, portrait);
            character.Init();
            return character;
        }

        public static Character Create(string guid, CharacterGameObjectData gameObjectData, Texture2D texture, Texture2D faceMeshTexture, Sprite portrait)
        {
            var character = new Character(guid, gameObjectData, texture, faceMeshTexture, portrait);
            character.Init();
            return character;
        }

        private Character(CharacterSaveData data, CharacterGameObjectData gameObjectData, Texture2D texture, Texture2D faceMeshTexture, Sprite portrait) : base(data, gameObjectData, texture)
        {
            FaceMeshTexture = faceMeshTexture;
            inventoryCeils = data.inventoryCeils;
            Portrait = portrait;
        }

        private Character(string guid, CharacterGameObjectData gameObjectData, Texture2D texture, Texture2D faceMeshTexture, Sprite portrait) : base(guid, gameObjectData, texture)
        {
            FaceMeshTexture = faceMeshTexture;
            Portrait = portrait;
        }

        protected override void Die()
        {
            base.Die();
            if (OnDied != null) OnDied(this);


            FSM.SpawnEvent((int)CharacterFSM.CharacterStateType.Dead);
        }

        protected override void Init()
        {
            base.Init();
            AttackManager = new CharacterAttackManager(this);
            MoveComponent = EntityGameObject.GetComponent<PlayerMoveComponent>();

            FSM = new CharacterFSM(this);
            FSM.Start();

            var canvas = EntityGameObject.GetComponentInChildren<EntityCanvas>();
            if (canvas != null) canvas.Init(this);
        }


        public void StartBattle()
        {
            FSM.SpawnEvent((int)CharacterFSM.CharacterStateType.Battle);
        }

        public bool IsTurnComplete()
        {
            return FSM.IsTurnComplete();
        }

        public void StartTurn(List<IBattleEntity> entities)
        {
            FSM.StartTurn(entities);
        }

        public void ProcessTurn()
        {
            FSM.ProcessTurn();
        }

        public int CalculateAP(float distance, IAttacked attackedEntity = null)
        {
            var movePoints = Mathf.CeilToInt(distance / Speed);
            if (attackedEntity == null || AttackManager == null) return movePoints;

            var attackPoints = 1; //todo
            return movePoints + attackPoints;
        }


        #region Equip
        public bool IsEquip(EquipItem item)
        {
            foreach (var equipPair in EquipItems)
                if (equipPair.Value.Guid == item.Guid)
                    return true;

            return false;
        }

        public EquipItemSlot EquipItem(EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined)
        {
            slotType = UnequipOldItems(item, slotType);
            if (slotType == EquipItemSlot.Undefined) return slotType;

            EquipItems[slotType] = item;
            if (OnEquipItem != null) OnEquipItem(item, slotType);

            RemoveFromInvetory(item);

            foreach (var statPair in item.Stats)
            {
                foreach (var modifier in statPair.Value)
                    StatCollection.AddStatModifier(statPair.Key, modifier, false);
            }
            StatCollection.UpdateStatModifiers();

            return slotType;
        }

        private EquipItemSlot UnequipOldItems(EquipItem item, EquipItemSlot slotType)
        {
            if (item.Data.itemType == EquipItemType.Weapon)
            {
                // If new item is 2 hand
                if (item.IsTwoHandItem)
                {
                    UnEquipItem(EquipItemSlot.HandRight);
                    UnEquipItem(EquipItemSlot.HandLeft);

                    return item.Data.itemSubType == EquipItemSubType.Bow ? EquipItemSlot.HandLeft : EquipItemSlot.HandRight;
                }

                // If old item is 2 hand
                if (EquipItems.ContainsKey(EquipItemSlot.HandRight) && EquipItems[EquipItemSlot.HandRight].ItemSubType == EquipItemSubType.TwoHand)
                {
                    UnEquipItem(EquipItemSlot.HandRight);
                    return slotType != EquipItemSlot.Undefined ? slotType : EquipItemSlot.HandRight;
                }

                // If old item is 2 bow
                if (EquipItems.ContainsKey(EquipItemSlot.HandLeft) && EquipItems[EquipItemSlot.HandLeft].ItemSubType == EquipItemSubType.Bow)
                {
                    UnEquipItem(EquipItemSlot.HandLeft);
                    return slotType != EquipItemSlot.Undefined ? slotType : EquipItemSlot.HandLeft;
                }

                if (slotType != EquipItemSlot.Undefined)
                {
                    if (!EquipItems.ContainsKey(slotType)) return slotType;
                    UnEquipItem(slotType);
                    return slotType;
                }
                else
                {
                    // Find free slot
                    if (!EquipItems.ContainsKey(EquipItemSlot.HandRight)) return EquipItemSlot.HandRight;
                    if (!EquipItems.ContainsKey(EquipItemSlot.HandLeft)) return EquipItemSlot.HandLeft;

                    UnEquipItem(EquipItemSlot.HandRight);
                    return EquipItemSlot.HandRight;
                }
            }

            // If shield unequip 2hand weapon
            if (item.ItemType == EquipItemType.Shield)
            {
                if ((EquipItems.ContainsKey(EquipItemSlot.HandRight) &&
                     EquipItems[EquipItemSlot.HandRight].ItemSubType == EquipItemSubType.TwoHand) ||
                    (EquipItems.ContainsKey(EquipItemSlot.HandLeft) &&
                     EquipItems[EquipItemSlot.HandLeft].ItemSubType == EquipItemSubType.Bow))
                {
                    UnEquipItem(EquipItemSlot.HandRight);
                    UnEquipItem(EquipItemSlot.HandLeft);
                    return EquipItemSlot.HandLeft;
                }
            }


            var slots = Helper.GetSlotsByItemType(item.ItemType);
            if (slots.Length > 1)
            {
                // Find Free slot
                foreach (var neededSlot in slots)
                {
                    if (!EquipItems.ContainsKey(neededSlot))
                    {
                        slotType = neededSlot;
                        break;
                    }
                }
            }
            else
            {
                slotType = slots[0];
            }

            UnEquipItem(slotType);
            return slotType;
        }

        public void UnEquipItem(EquipItem equipItem)
        {
            var slotType = EquipItemSlot.Undefined;
            foreach (var itemPair in EquipItems)
            {
                if (itemPair.Value.Guid != equipItem.Guid) continue;

                slotType = itemPair.Key;
                break;
            }

            UnEquipItem(slotType, false);
        }

        private void UnEquipItem(EquipItemSlot slotType, bool addToInventory = true)
        {
            if (!EquipItems.ContainsKey(slotType)) return;

            var item = EquipItems[slotType];
            EquipItems.Remove(slotType);
            if (addToInventory && OnAddToInventory != null) OnAddToInventory(guid, item);  //todo remove can unequip to ground, inventory, container and etc
            if (OnUnequipItem != null) OnUnequipItem(item);

            foreach (var statPair in item.Stats)
            {
                foreach (var modifier in statPair.Value)
                    StatCollection.RemoveStatModifier(statPair.Key, modifier, false);
            }
            StatCollection.UpdateStatModifiers();
        }

        public void AddFaceMesh(FaceMesh item)
        {
            //Equip new item
            FaceMeshItems[item.MeshType] = item;
            item.Equip(GameObjectData.meshBones, GameObjectData.previewMeshBones);
        }

        public void RemoveFromInvetory(Item item)
        {
            if (OnRemoveFromInventory != null) OnRemoveFromInventory(guid, item);
        }

    

        public void SwapItems(EquipItemSlot slot1, EquipItemSlot slot2)
        {
            if (!EquipItems.ContainsKey(slot1) || !EquipItems.ContainsKey(slot2)) return;

            var tmp = EquipItems[slot1];
            EquipItems[slot1] = EquipItems[slot2];
            EquipItems[slot2] = tmp;
        }

        public EquipItem GetWeapon()
        {
            EquipItem item;
            if (EquipItems.TryGetValue(EquipItemSlot.HandRight, out item) &&
                item.ItemType == EquipItemType.Weapon) return item;

            if (EquipItems.TryGetValue(EquipItemSlot.HandLeft, out item) &&
                item.ItemType == EquipItemType.Weapon) return item;

            return null;
        }

#endregion
    }
}
