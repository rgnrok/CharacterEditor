using System.Collections.Generic;

namespace StatSystem
{
    public class DefaultStatCollection : StatCollection
    {
        public DefaultStatCollection():this(100, 100, new Dictionary<StatType, int>())
        {

        }

        public DefaultStatCollection(int healthValue, int manaValue, Dictionary<StatType, int> statDir)
        {
            var stamValue = statDir.ContainsKey(StatType.Stamina) ? statDir[StatType.Stamina] : 10;
            var stamina = CreateOrGetStat<Attribute>(StatType.Stamina, stamValue);
            stamina.StatName = "Stamina";

            var wisdomVal = statDir.ContainsKey(StatType.Wisdom) ? statDir[StatType.Wisdom] : 10;
            var wisdom = CreateOrGetStat<Attribute>(StatType.Wisdom, wisdomVal);
            wisdom.StatName = "Wisdom";

            var strengthVal = statDir.ContainsKey(StatType.Strength) ? statDir[StatType.Strength] : 10;
            var strength = CreateOrGetStat<Attribute>(StatType.Strength, strengthVal);
            strength.StatName = "Strength";

            var agilityVal = statDir.ContainsKey(StatType.Agility) ? statDir[StatType.Agility] : 10;
            var agility = CreateOrGetStat<Attribute>(StatType.Agility, agilityVal);
            agility.StatName = "Agility";

            var intellectVal = statDir.ContainsKey(StatType.Intellect) ? statDir[StatType.Intellect] : 10;
            var intellect = CreateOrGetStat<Attribute>(StatType.Intellect, intellectVal);
            intellect.StatName = "Intellect";

            var healtVal = statDir.ContainsKey(StatType.Health) ? statDir[StatType.Health] : 10;
            var health = CreateOrGetStat<Vital>(StatType.Health, healtVal);
            health.StatName = "Health";
            health.AddLinker(new StatLinkerBasic(stamina, 5));
            health.UpdateLinkers();
            health.StatCurrentValue= healthValue;

            var manaVal = statDir.ContainsKey(StatType.Mana) ? statDir[StatType.Mana] : 10;
            var mana = CreateOrGetStat<Vital>(StatType.Mana, manaVal);
            mana.StatName = "Mana";
            mana.AddLinker(new StatLinkerBasic(wisdom, 5));
            mana.UpdateLinkers();
            mana.StatCurrentValue = manaValue;


            var actionPointsValue = statDir.ContainsKey(StatType.ActionPoint) ? statDir[StatType.ActionPoint] : 6;
            var actionPoints = CreateOrGetStat<Vital>(StatType.ActionPoint, actionPointsValue);
            actionPoints.StatName = "ActionPoints";

            var speedVal = statDir.ContainsKey(StatType.Speed) ? statDir[StatType.Speed] : 130;
            var speed = CreateOrGetStat<Attribute>(StatType.Speed, speedVal);
            speed.StatName = "Speed";
        }
    }
}