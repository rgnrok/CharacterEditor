using System;
using System.Collections.Generic;
using StatSystem;
using Random = UnityEngine.Random;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class EquipItem : Item
        {
            public EquipItemMesh ItemMesh { get; private set; }

            public readonly Dictionary<StatType, List<StatModifier>> Stats = new Dictionary<StatType, List<StatModifier>>();

            public EquipItemType ItemType
            {
                get { return Data.itemType; }
            }

            public EquipItemSubType ItemSubType
            {
                get { return Data.itemSubType; }
            }

            public bool IsTwoHandItem
            {
                get { return Data.itemSubType == EquipItemSubType.Bow || Data.itemSubType == EquipItemSubType.TwoHand;}
            }
           
            private EquipItemData _equipItemData;
            public new EquipItemData Data
            {
                get
                {
                    if (_equipItemData == null) _equipItemData = base.Data as EquipItemData;
                    return _equipItemData;
                }
            }

            private string _guid;
            public override string Guid
            {
                get { return _guid; }
            }

            public string DataGuid
            {
                get { return Data.guid; }
            }
            
            public EquipItem(ItemData itemData, EquipItemMesh eiMesh) : this(null, itemData, eiMesh, new StatData[0])
            {
                if (Data.randomStats)
                {
                    var statTypes = Enum.GetValues(typeof(StatType));
                    var modifierTypes = Enum.GetValues(typeof(ModifierType));
                    for (int i = 0; i < Data.randomStatsCount; i++)
                    {
                        var type = (StatType) Random.Range(1, (int) statTypes.GetValue(statTypes.Length - 1) + 1);
                        var modType = (ModifierType)Random.Range(1, (int)modifierTypes.GetValue(modifierTypes.Length - 1) + 1);

                        if (!Stats.ContainsKey(type)) Stats[type] = new List<StatModifier>();

                        Stats[type].Add(ModifierFactory.Create(modType, Random.Range(1, 5)));
                    }
                }
                else
                {
                    foreach (var statData in Data.stats)
                    {
                        var type = statData.type;

                        if(!Stats.ContainsKey(type)) Stats[type] = new List<StatModifier>();
                        Stats[type].Add(ModifierFactory.Create(statData.modType, statData.value));
                    }
                }
            }

            public EquipItem(string guid, ItemData itemData, EquipItemMesh eiMesh, StatData[] stats) : base(itemData)
            {
                _guid = guid ?? System.Guid.NewGuid().ToString();
                ItemMesh = eiMesh;

                foreach (var statInfo in stats)
                {
                    if (!Stats.ContainsKey(statInfo.type))
                        Stats[statInfo.type] = new List<StatModifier>();

                    Stats[statInfo.type].Add(ModifierFactory.Create(statInfo.modType, statInfo.value));
                }
            }
        }
    }
}