using System;
using CharacterEditor;

public interface IFSM : IService
{
    void Start();
    void SpawnEvent<T>(int transitionId, T param);
    void SpawnEvent(int transitionId);
    void Update();

    IExitableState CurrentState { get; }
    event Action<IExitableState> OnCurrentStateChanged;
}