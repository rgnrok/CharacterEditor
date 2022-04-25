using System;
using System.Collections.Generic;

namespace StatSystem
{
    public class StatCollection
    {
        private Dictionary<StatType, Stat> _statDict;
        public Dictionary<StatType, Stat> StatDict
        {
            get
            {
                if (_statDict == null) _statDict = new Dictionary<StatType, Stat>();
                return _statDict;
            }
        }

        public Action UpdateStats;

  
        protected virtual void ConfigureStats()
        {

        }

        public bool ContainStat(StatType statType)
        {
            return StatDict.ContainsKey(statType);
        }

        public Stat GetStat(StatType statType)
        {
            if (ContainStat(statType))
                return StatDict[statType];

            return null;
        }

        public T GetStat<T>(StatType type) where T : Stat
        {
            return GetStat(type) as T;
        }

        protected T CreateStat<T>(StatType statType, int value) where T : Stat
        {
            T stat = (T)System.Activator.CreateInstance(typeof(T), value);
            StatDict.Add(statType, (Stat) stat);
            return stat;
        }

        protected T CreateOrGetStat<T>(StatType statType, int defaultVal) where T : Stat
        {
            T stat = GetStat<T>(statType);
            if (stat == null)
            {
                stat = CreateStat<T>(statType, defaultVal);
            }

            return stat;
        }

        public void AddStatModifier(StatType target, StatModifier mod, bool update = true)
        {
            if (ContainStat(target))
            {
                var modStat = GetStat(target) as IStatModifiable;
                if (modStat != null)
                {
                    modStat.AddModifier(mod);
                    if (update)
                    {
                        modStat.UpdateModifiers();
                    }
                }
            }

            if (update && UpdateStats != null) UpdateStats();
        }

        public void RemoveStatModifier(StatType target, StatModifier mod, bool update = true)
        {
            if (ContainStat(target))
            {
                var modStat = GetStat(target) as IStatModifiable;

                if (modStat != null)
                {
                    modStat.RemoveModifier(mod);
                    if (update)
                    {
                        modStat.UpdateModifiers();
                    }
                }
            }

            if (update && UpdateStats != null) UpdateStats();
        }

        public void ClearAllStatModifiers(bool update = true)
        {
            foreach (var key in StatDict.Keys)
            {
                ClearStatModifier(key, update);
            }
        }

        public void ClearStatModifier(StatType target, bool update = true)
        {
            if (ContainStat(target))
            {
                var modStat = GetStat(target) as IStatModifiable;
                if (modStat != null)
                {
                    modStat.ClearModifiers();
                    if (update)
                    {
                        modStat.UpdateModifiers();
                    }
                }
            }
            if (update && UpdateStats != null) UpdateStats();
        }


        public void UpdateStatModifiers()
        {
            foreach (var key in StatDict.Keys)
            {
                UpdateStatModifer(key);
            }
            if (UpdateStats != null) UpdateStats();
        }


        public void UpdateStatModifer(StatType target)
        {
            if (ContainStat(target))
            {
                var modStat = GetStat(target) as IStatModifiable;
                if (modStat != null)
                {
                    modStat.UpdateModifiers();
                }
            }
        }


        public void ScaleStatCollection(int level)
        {
            foreach (var key in StatDict.Keys)
            {
                ScaleStat(key, level);
            }
        }


        public void ScaleStat(StatType target, int level)
        {
            if (ContainStat(target))
            {
                var stat = GetStat(target) as IStatScalable;
                if (stat != null)
                {
                    stat.ScaleStat(level);
                }
            }
            if (UpdateStats != null) UpdateStats();
        }
    }
}