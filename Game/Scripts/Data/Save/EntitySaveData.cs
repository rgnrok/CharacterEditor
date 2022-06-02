using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StatSystem;

[Serializable]
public abstract class EntitySaveData : ISerializable
{
    public string guid;
    public string configGuid;

    public SerializableVector3 position;
    public SerializableQuaternion rotation;

    public int currentHealthValue;
    public int currentManaValue;
    public Dictionary<StatType, int> stats;

    public StatCollection GetStats()
    {
        return new DefaultStatCollection(currentHealthValue, currentManaValue, stats);
    }

    public void UpdateStats(StatCollection statCollection)
    {
        stats = new Dictionary<StatType, int>();
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            if (!statCollection.ContainStat(statType)) continue;
            stats[statType] = statCollection.GetStat<Stat>(statType).StatBaseValue;
        }

        var health = statCollection.GetStat<Vital>(StatType.Health);
        if (health != null) currentHealthValue = health.StatCurrentValue;

        var mana = statCollection.GetStat<Vital>(StatType.Mana);
        if (mana != null) currentManaValue = mana.StatCurrentValue;
    }

    protected EntitySaveData() { }

    protected EntitySaveData(SerializationInfo info, StreamingContext context)
    {
        guid = info.GetString("guid");
        configGuid = info.GetString("configPath");
        position = (SerializableVector3)info.GetValue("position", typeof(SerializableVector3));
        rotation = (SerializableQuaternion)info.GetValue("rotation", typeof(SerializableQuaternion));
        currentHealthValue = info.GetInt32("currentHealthValue");
        currentManaValue = info.GetInt32("currentManaValue");
        stats = (Dictionary<StatType, int>)info.GetValue("stats", typeof(Dictionary<StatType, int>));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("guid", guid);
        info.AddValue("configPath", configGuid);
        info.AddValue("position", position);
        info.AddValue("rotation", rotation);
        info.AddValue("currentHealthValue", currentHealthValue);
        info.AddValue("currentManaValue", currentManaValue);
        info.AddValue("stats", stats);
    }
}
