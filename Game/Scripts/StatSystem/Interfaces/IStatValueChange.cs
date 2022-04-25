using System;

namespace StatSystem
{
    public interface IStatValueChange
    {
        event Action OnValueChange;
    }
}