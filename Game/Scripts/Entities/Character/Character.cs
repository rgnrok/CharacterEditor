using System;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using StatSystem;
using UnityEngine;
using Attribute = StatSystem.Attribute;

namespace CharacterEditor
{
    public class Character: Entity<CharacterGameObjectData, CharacterConfig>, IBattleEntity, IAttacked, IHover, IEquatable<Character>
    {
        public readonly Dictionary<EquipItemSlot, EquipItem> EquipItems = new Dictionary<EquipItemSlot, EquipItem>();
        public readonly Dictionary<MeshType, FaceMesh> FaceMeshItems = new Dictionary<MeshType, FaceMesh>();
        public readonly Dictionary<int, CharacterItemData> InventoryCells = new Dictionary<int, CharacterItemData>();

        public Texture2D FaceMeshTexture { get; }
        public Sprite Portrait { get; }

        private CharacterFSM _characterFSM;
        public IFSM FSM => _characterFSM;

        public CharacterAttackComponent AttackComponent { get; private set; }
        public PlayerMoveComponent MoveComponent { get; private set; }

        public Vital ActionPoints { get { return StatCollection.GetStat<Vital>(StatType.ActionPoint); } }
        public float Speed { get { return StatCollection.GetStat<Attribute>(StatType.Speed).StatValue / 100f; } }

        public event Action<EquipItem, EquipItemSlot> OnEquipItem;
        public event Action<EquipItem> OnUnEquipItem;

        public event Action<string, Item> OnAddToInventory;
        public event Action<string, Item> OnRemoveFromInventory;

        public event Action<IBattleEntity> OnDied;

        public Character(string guid, StatCollection stats, CharacterGameObjectData gameObjectData, Texture2D texture, Texture2D faceMeshTexture, Sprite portrait) : base(guid, gameObjectData, texture, stats)
        {
            FaceMeshTexture = faceMeshTexture;
            Portrait = portrait;
        }

        public Character(CharacterSaveData data, CharacterGameObjectData gameObjectData, Texture2D texture, Texture2D faceMeshTexture, Sprite portrait) : this(data.guid, data.GetStats(), gameObjectData, texture, faceMeshTexture, portrait)
        {
            InventoryCells = data.inventoryCells;
        }

        protected override void OnDie()
        {
            base.OnDie();
            OnDied?.Invoke(this);

            _characterFSM.SpawnEvent((int)CharacterFSM.CharacterStateType.Dead);
        }

        protected override void InternalInit()
        {
            base.InternalInit();

            AttackComponent = new CharacterAttackComponent(this);
            MoveComponent = EntityGameObject.GetComponent<PlayerMoveComponent>();

            _characterFSM = new CharacterFSM(this);
            _characterFSM.Start();

            var canvas = EntityGameObject.GetComponentInChildren<EntityCanvas>();
            if (canvas != null) canvas.Init(this);
        }

        #region Equials

        public static bool operator ==(Character ch1, Character ch2)
        {
            if (ReferenceEquals(ch1, ch2))
                return true;
            if (ch1 is null || ch2 is null)
                return false;
            return ch1.Equals(ch2);
        }

        public static bool operator !=(Character ch1, Character ch2)
        {
            return !(ch1 == ch2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Character character)) return false;
            return Equals(character);
        }

        public bool Equals(Character other)
        {
            if (other is null) return false;
            return Guid == other.Guid;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        #endregion


        #region IBattleEntity

        public void StartBattle()
        {
            _characterFSM.SpawnEvent((int)CharacterFSM.CharacterStateType.Battle);
        }

        public bool IsTurnComplete()
        {
            return _characterFSM.IsTurnComplete();
        }

        public void StartTurn(List<IBattleEntity> entities)
        {
            _characterFSM.StartTurn(entities);
        }

        public void ProcessTurn()
        {
            _characterFSM.ProcessTurn();
        }

        #endregion

        public int CalculateAP(float distance, IAttacked attackedEntity = null)
        {
            var movePoints = Mathf.CeilToInt(distance / Speed);
            if (attackedEntity == null || AttackComponent == null) return movePoints;

            var attackPoints = 1; //todo
            return movePoints + attackPoints;
        }

        #region Equip
        public bool IsEquip(EquipItem item)
        {
            foreach (var equipPair in EquipItems)
                if (equipPair.Value.Guid == item.Guid) return true;

            return false;
        }

        public EquipItemSlot EquipItem(EquipItem item, EquipItemSlot slotType)
        {
            slotType = UnEquipOldItem(item, slotType);
            if (slotType == EquipItemSlot.Undefined)
                slotType = GetAvailableSlot(item);

            if (slotType == EquipItemSlot.Undefined) return slotType;

            EquipItems[slotType] = item;
            OnEquipItem?.Invoke(item, slotType);

            RemoveFromInventory(item);

            foreach (var statPair in item.Stats)
            {
                foreach (var modifier in statPair.Value)
                    StatCollection.AddStatModifier(statPair.Key, modifier, false);
            }
            StatCollection.UpdateStatModifiers();

            return slotType;
        }

        private EquipItemSlot GetAvailableSlot(EquipItem item)
        {
            var slots = Helper.GetAvailableSlotsByItemType(item.ItemType);
            if (slots.Length == 0) return EquipItemSlot.Undefined;
            if (slots.Length == 1) return slots[0];

            // Find Free slot
            foreach (var neededSlot in slots)
            {
                if (!EquipItems.ContainsKey(neededSlot))
                    return neededSlot;
            }

            return EquipItemSlot.Undefined;
        }

        private EquipItemSlot UnEquipOldItem(EquipItem newItem, EquipItemSlot newItemSlot)
        {
            if (newItem.ItemType == EquipItemType.Weapon)
                return UnEquipOldWeapon(newItem, newItemSlot);

            if (newItem.ItemType == EquipItemType.Shield)
                return UnEquipOldShield();

            UnEquipItem(newItemSlot);
            return newItemSlot;
        }

        private EquipItemSlot UnEquipOldWeapon(EquipItem newItem, EquipItemSlot newItemSlot)
        {
            // If new item is 2 hand
            if (newItem.IsTwoHandItem)
            {
                UnEquipItem(EquipItemSlot.HandRight);
                UnEquipItem(EquipItemSlot.HandLeft);

                return newItem.ItemSubType == EquipItemSubType.Bow ? EquipItemSlot.HandLeft : EquipItemSlot.HandRight;
            }

            // If old item is 2 hand
            if (EquipItems.ContainsKey(EquipItemSlot.HandRight) && EquipItems[EquipItemSlot.HandRight].ItemSubType == EquipItemSubType.TwoHand)
            {
                UnEquipItem(EquipItemSlot.HandRight);
                return newItemSlot != EquipItemSlot.Undefined ? newItemSlot : EquipItemSlot.HandRight;
            }

            // If old item is 2 bow
            if (EquipItems.ContainsKey(EquipItemSlot.HandLeft) && EquipItems[EquipItemSlot.HandLeft].ItemSubType == EquipItemSubType.Bow)
            {
                UnEquipItem(EquipItemSlot.HandLeft);
                return newItemSlot != EquipItemSlot.Undefined ? newItemSlot : EquipItemSlot.HandLeft;
            }

            if (newItemSlot != EquipItemSlot.Undefined)
            {
                if (!EquipItems.ContainsKey(newItemSlot)) return newItemSlot;
                UnEquipItem(newItemSlot);
                return newItemSlot;
            }

            // Find free slot
            if (!EquipItems.ContainsKey(EquipItemSlot.HandRight)) return EquipItemSlot.HandRight;
            if (!EquipItems.ContainsKey(EquipItemSlot.HandLeft)) return EquipItemSlot.HandLeft;

            UnEquipItem(EquipItemSlot.HandRight);
            return EquipItemSlot.HandRight;
        }

        private EquipItemSlot UnEquipOldShield()
        {
            if (EquipItems.ContainsKey(EquipItemSlot.HandRight) && EquipItems[EquipItemSlot.HandRight].ItemSubType == EquipItemSubType.TwoHand)
                UnEquipItem(EquipItemSlot.HandRight);

            UnEquipItem(EquipItemSlot.HandLeft);
            return EquipItemSlot.HandLeft;

        }

        public bool UnEquipItem(EquipItem equipItem)
        {
            var slotType = EquipItemSlot.Undefined;
            foreach (var itemPair in EquipItems)
            {
                if (itemPair.Value.Guid != equipItem.Guid) continue;

                slotType = itemPair.Key;
                break;
            }

            if (slotType == EquipItemSlot.Undefined) return false;

            return UnEquipItem(slotType, false);
        }

        private bool UnEquipItem(EquipItemSlot slotType, bool addToInventory = true)
        {
            if (!EquipItems.ContainsKey(slotType)) return false;

            var item = EquipItems[slotType];
            EquipItems.Remove(slotType);

            if (addToInventory && OnAddToInventory != null) OnAddToInventory(Guid, item);  //todo remove can unequip to ground, inventory, container and etc
            OnUnEquipItem?.Invoke(item);

            foreach (var statPair in item.Stats)
            {
                foreach (var modifier in statPair.Value)
                    StatCollection.RemoveStatModifier(statPair.Key, modifier, false);
            }
            StatCollection.UpdateStatModifiers();
            return true;
        }

        public void AddFaceMesh(FaceMesh item)
        {
            //Equip new item
            FaceMeshItems[item.MeshType] = item;
            item.Equip(GameObjectData.meshBones, GameObjectData.previewMeshBones);
        }

        private void RemoveFromInventory(Item item)
        {
            OnRemoveFromInventory?.Invoke(Guid, item);
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
            if (EquipItems.TryGetValue(EquipItemSlot.HandRight, out var item) &&
                item.ItemType == EquipItemType.Weapon) return item;

            if (EquipItems.TryGetValue(EquipItemSlot.HandLeft, out item) &&
                item.ItemType == EquipItemType.Weapon) return item;

            return null;
        }

#endregion


    }
}
