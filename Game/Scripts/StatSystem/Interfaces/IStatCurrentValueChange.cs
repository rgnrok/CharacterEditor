using System;

namespace StatSystem
{
    public interface IStatCurrentValueChange
    {
        event Action OnCurrentValueChange;
    }
}