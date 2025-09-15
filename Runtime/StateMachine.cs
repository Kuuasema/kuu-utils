using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kuuasema.Utils {
    
    /**
    * Generic state machine. 
    */
    public class StateMachine<T> where T : Enum {
        /**
        * Class to derive state implementations from
        */
        public class State {
            // allow update to pass down below this state in the stack
            public bool PassUpdateDown { get; set; }
            public bool Initialized { get; private set; }
            public void Initialize() {
                if (!this.Initialized) {
                    this.Initialized = true;
                    this.OnStateInitialize();
                }
            }
            public virtual void UpdateTopState() {
                // this is the main state update that will be called only on the topmost state
            }
            public virtual void UpdateDownState() {
                // this will be called for states down the stack if the update passes down to them
            }
            protected virtual void OnStateInitialize() {}
            public virtual void OnStateEnter() {}
            public virtual void OnStateExit() {}
            public virtual void OnStateUp() {}
            public virtual void OnStateDown() {}
        }
        // the states can stack
        public List<T> StateStack { get; private set; } = new List<T>();
        public T TopState => StateStack[^1];
        public Dictionary<T,State> StateMap { get; private set; } = new Dictionary<T,State>();
        public Queue<T> StateQueue { get; private set; } = new Queue<T>();

        /**
        * Creates a state of given type.
        */
        public void CreateState<K>(T key) where K : State, new() {
            if (RuntimeUtils.IsRunning) {
                // use pools on runtime
                this.AssignState(key, GenericPool<K>.Get());
            } else {
                this.AssignState(key, Activator.CreateInstance<K>());
            }
        }

        /**
        * Adds the state to the mapping
        */
        public StateMachine<T> AssignState(T key, State val) {
            this.StateMap[key] = val;
            return this;
        }

        public bool HasStateAssigned(T key) {
            return this.StateMap.ContainsKey(key);
        }

        public State GetStateAssigned(T key) {
            return this.StateMap.GetValueOrDefault(key);
        }

        public void Reset() {
            this.StateStack.Clear();
            this.StateQueue.Clear();
        }

        private bool inUpdate;

        public T UpdateStateMachine() {
            // if current stack is empty, then it is time to check the queue, and abort if nothing
            if (this.StateStack.Count == 0 && !this.TryStateDequeue()) return default(T);
            this.inUpdate = true;

            // update topmost state, and the trickle down and update for as long as the states permit
            State topState = this.StateMap[TopState];
            topState.UpdateTopState();
            if (topState.PassUpdateDown) {
                for (int i = this.StateStack.Count - 2; i >= 0; i--) {
                    T down = this.StateStack[i];
                    State downState = this.StateMap[down];
                    downState.UpdateDownState();
                    if (!downState.PassUpdateDown) break;
                }
            }
            this.inUpdate = false;
            return TopState;
        }

        private bool TryStateDequeue() {
            if (this.StateQueue.Count > 0) {
                T _state = this.StateQueue.Dequeue();
                this.StateStack.Add(_state);
                State state = this.StateMap[_state];
                if (!state.Initialized) {
                    state.Initialize();
                }
                state.OnStateEnter();
                return true;
            }
            return false;
        }

        /**
        * Queuing a state will activate it when the state machine runs out its current stack
        */
        public void QueueState(T state) {
            this.StateQueue.Enqueue(state);
        }

        public bool IsStatePushing { get; private set; }
        public bool IsStateInserting { get; private set; }
        public bool IsStatePopping { get; private set; }
        public T NextState { get; private set; }
        public T InsertingState { get; private set; }
        public int InsertingStateIndex { get; private set; }

        /**
        * States can be pushed on top of each other.
        * 25.07.2023 Jaakko, made this private to enforce the use of TryPushState instead
        */
        private void PushState(T _state) {
            if (this.inUpdate) {
                this.PushStateDeferred(_state);
                return;
            }
            this.IsStatePushing = true;
            this.NextState = _state;
            if (this.StateStack.Count > 0) {
                this.StateMap[TopState].OnStateDown();
            }
            this.StateStack.Add(_state);
            State state = this.StateMap[_state];
            if (!state.Initialized) {
                state.Initialize();
            }
            this.IsStatePushing = false;
            state.OnStateEnter();
        }

        public bool TryPushState(T _state) {
            if (this.IsStatePushing) {
                if (EqualityComparer<T>.Default.Equals(this.NextState, _state)) {
                    Debug.LogWarning($"TryPushState({_state}) while: IsStatePushing = {this.IsStatePushing}, NextState = {this.NextState}, IsStatePopping = {this.IsStatePopping})");
                } else {
                    Debug.LogError($"TryPushState({_state}) while: IsStatePushing = {this.IsStatePushing}, NextState = {this.NextState}, IsStatePopping = {this.IsStatePopping})");
                }
                return false;
            }
            
            this.PushStateDeferred(_state);
            return true;
        }

        public void PushStateDeferred(T _state) {
            this.IsStatePushing = true;
            this.NextState = _state;
            ScheduledUpdater.RequestLateUpdate(() => this.PushState(_state));
        }

        /**
        * States can be inserted into the list.
        * 09.09.2025 Jakub, added this to enable queueing states
        */
        private void InsertState(T _state, int index)
        {
            if (this.inUpdate)
            {
                this.InsertStateDeferred(_state, index);
                return;
            }
            this.IsStateInserting = true;
            this.InsertingState = _state;
            this.StateStack.Insert(index, _state);
            State state = this.StateMap[_state];
            if (!state.Initialized)
            {
                state.Initialize();
            }
            this.IsStateInserting = false;
            if (StateStack.Count-1 <= index)
            {
                state.OnStateEnter();
            }
        }

        public bool TryInsertState(T _state, int index)
        {
            if (this.IsStateInserting)
            {
                if (EqualityComparer<T>.Default.Equals(this.InsertingState, _state))
                {
                    Debug.LogWarning($"TryInsertState({_state}) while: IsStateInserting = {this.IsStateInserting}, InsertingState = {this.InsertingState}, InsertingStateIndex = {this.InsertingStateIndex})");
                }
                else
                {
                    Debug.LogError($"TryInsertState({_state}) while: IsStateInserting = {this.IsStateInserting}, InsertingState = {this.InsertingState}, InsertingStateIndex = {this.InsertingStateIndex})");
                }
                return false;
            }

            this.InsertStateDeferred(_state, index);
            return true;
        }

        public void InsertStateDeferred(T _state, int index)
        {
            this.IsStateInserting = true;
            this.InsertingState = _state;
            this.InsertingStateIndex = index;
            ScheduledUpdater.RequestLateUpdate(() => this.InsertState(_state, index));
        }

        /**
        * States can be popped
        */
        private void PopState() {
            if (this.inUpdate) {
                ScheduledUpdater.RequestLateUpdate(this.PopState);
                return;
            }
            this.IsStatePopping = true;
            if (this.StateStack.Count > 0) {
                this.StateMap[TopState].OnStateExit();
                this.StateStack.RemoveAt(this.StateStack.Count - 1);
            }
            this.NextState = default(T);
            if (this.StateStack.Count > 0) {
                this.StateMap[TopState].OnStateUp();
                this.NextState = TopState;
            }
            this.IsStatePopping = false;
        }

        public bool TryPopState() {
            // Debug.Log($"TryPopState while: IsStatePushing = {this.IsStatePushing}, IsStatePopping = {this.IsStatePopping})");
            if (this.IsStatePushing || this.IsStatePopping) return false;
            this.PopStateDeferred();
            return true;
        }

        public void PopStateDeferred() {
            this.IsStatePopping = true;
            ScheduledUpdater.RequestLateUpdate(() => this.PopState());
        }

        /**
        * Change state will pop the existing topmost state and then push the new state
        */
        public void ChangeState(T state) {
            this.TryPopState();
            this.TryPushState(state);
        }

        /**
        * Clear to state will clear the state stack (and queue) and begin from the given new state
        */
        public void ClearToState(T state) {
            if (this.IsStatePushing || this.IsStatePopping) {
                if (this.NextState.Equals(state) || this.StateStack[0].Equals(state)) {
                    // harmless attempt to change state into what it is currently changing to or already cleared to
                    return;
                }
                Debug.LogError($"Cannot clear to state {state} while already: pushing = {this.IsStatePushing}, popping = {this.IsStatePopping}, next state = {this.NextState}");
                return;
            }
            if (this.inUpdate) {
                this.ClearToStateDeferred(state);
                return;
            }
            for (int i = this.StateStack.Count - 1; i >= 0; i--) {
                this.StateMap[this.StateStack[i]].OnStateExit();
            }
            this.StateStack.Clear();
            // we also have to clear the queue, else the new state will have unknown states following
            // which leads to undefined state flow
            this.StateQueue.Clear();
            this.PushState(state);
        }

        public void ClearToStateDeferred(T state) {
            ScheduledUpdater.RequestLateUpdate(() => this.ClearToState(state));
        }
    }

    /**
    * A static statemachine wrapper.
    */
    public class StaticStateMachine<T> where T : Enum {
        protected static StateMachine<T> instance { get; set; }
        protected static StateMachine<T> Instance { 
            get {
                if (instance == null) {
                    instance = new StateMachine<T>();
                }
                return instance;
            } 
        }
        public static void CreateState<K>(T key) where K : StateMachine<T>.State, new() {
            Instance.CreateState<K>(key);
        }
        public static StateMachine<T> AssignState(T key, StateMachine<T>.State val) {
            return Instance.AssignState(key, val);
        }
        public static void UpdateStateMachine() {
            Instance.UpdateStateMachine();
        }
        // public static void PushState(T state) {
        //     Instance.PushState(state);
        // }
        public static bool TryPushState(T state) {
            return Instance.TryPushState(state);
        }
        // public static void PopState() {
        //     Instance.PopState();
        // }
        public static bool TryPopState() {
            return Instance.TryPopState();
        }
        public static void ChangeState(T state) {
            Instance.ChangeState(state);
        }
        public static void ClearToState(T state) {
            Instance.ClearToState(state);
        }
    }
}