using UnityEngine;

namespace StatSystem
{
    public class DefaultLevel : EntityLevel
    {

        public override int GetExpRequiredForLevel(int level)
        {
            return (int)(Mathf.Pow(level, 2f) * 100);
        }
    }
}
