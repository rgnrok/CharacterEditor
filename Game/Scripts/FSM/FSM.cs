using System;
using System.Collections.Generic;
using CharacterEditor;

public abstract class FSM : IFSM
{
        private readonly Dictionary<Type, IExitableState> _states = new Dictionary<Type, IExitableState>();

        public event Action<IExitableState> OnCurrentStateChanged;

        private IUpdatableState _currentUpdatableState;
        private IExitableState _currentState;

        public IExitableState CurrentState
        {
            get => _currentState;
            private set
            {
                _currentState = value;
                FireOnCurrentStateChanged();
            }
        }

        private readonly List<FSMTransition> _transitions = new List<FSMTransition>();

        public abstract void Start();

        public void FireOnCurrentStateChanged()
        {
            OnCurrentStateChanged?.Invoke(CurrentState);
        }

        public void SpawnEvent<T>(int transitionId, T param)
        {
            foreach (var t in _transitions)
            {
                if (t.Id != transitionId) continue;
                if (t.From != null && t.From != CurrentState) continue;
                if (!(t.To is IPayloadedState<T> tState)) continue;

                SwitchT(tState, param);
                return;
            }

            Logger.LogWarning("Unit FSM: Event " + transitionId + " not handled");
        }

        public void SpawnEvent(int transitionId)
        {
            foreach (FSMTransition t in _transitions)
            {
                if (t.Id != transitionId) continue;
                if (t.From != null && t.From != CurrentState) continue;
                if (!(t.To is IState tState)) continue;

                Switch(tState);
                return;
            }

            Logger.LogWarning("Unit FSM: Event " + transitionId + " not handled");
        }
    
        public void Update()
        {
            if (_currentUpdatableState != null)
                _currentUpdatableState.Update();
        }

        protected T AddState<T>(T state) where T : IExitableState
        {
            _states[typeof(T)] = state;
            return state;
        }

        protected void AddGlobalTransition(int transition, IExitableState stateTo, IExitableState[] exclude = null)
        {
            foreach (var state in _states.Values)
            {
                if (exclude == null || Array.IndexOf(exclude, state) == -1)
                    _transitions.Add(new FSMTransition(transition, state, stateTo));
            }
        }

        protected void AddTransition(int transition, IExitableState stateFrom, IExitableState stateTo)
        {
            _transitions.Add(new FSMTransition(transition, stateFrom, stateTo));
        }

        private void SwitchT<TPayload>(IPayloadedState<TPayload> to, TPayload param)
        {
            CurrentState?.Exit();

            CurrentState = to;
            TryCheckUpdatableState();
            to.Enter(param);
        }

        protected void Switch(IState to)
        {
            if (CurrentState == to) return;

            CurrentState?.Exit();

            CurrentState = to;

            TryCheckUpdatableState();
            to.Enter();
        }

        private void TryCheckUpdatableState()
        {
            if (CurrentState is IUpdatableState updatableState) _currentUpdatableState = updatableState;
            else _currentUpdatableState = null;
        }
    }
