using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    public class ScheduledUpdater : MonoBehaviour {
        private static ScheduledUpdater instance;
        private static bool quitting;
        private static bool Create() {
            if (quitting) return false;
            instance = new GameObject("ScheduledUpdater").AddComponent<ScheduledUpdater>();
            DontDestroyOnLoad(instance.gameObject);
            return true;
        }

        private static List<Action> updates = new List<Action>();
        private static List<Action> lateUpdates = new List<Action>();
        private static List<Action> fixedUpdates = new List<Action>();
        private static List<Action> continuousUpdates = new List<Action>();
        private static List<Action> continuousLateUpdates = new List<Action>();
        private static List<Action> continuousFixedUpdates = new List<Action>();

        private void OnApplicationQuit() {
            quitting = true;
        }

        public static void RunCoroutine(IEnumerator coroutine) {
            if (instance == null) {
                if (!Create()) return;
            }
            instance.StartCoroutine(coroutine);
        }

        public static void RequestUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InUpdate) {
                // dont alter the main collections while inside the update
                addActions.Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (!updates.Contains(action)) {
                updates.Add(action);
            }
        }

        public static void RequestLateUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InLateUpdate) {
                // dont alter the main collections while inside the update
                addActions.Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (!lateUpdates.Contains(action)) {
                lateUpdates.Add(action);
            }
        }

        public static void RequestFixedUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InFixedUpdate) {
                // dont alter the main collections while inside the update
                addActions.Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (!fixedUpdates.Contains(action)) {
                fixedUpdates.Add(action);
            }
        }

        public static void RequestContinuousUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InUpdate) {
                // dont alter the main collections while inside the update
                addContinuousActions.Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (!continuousUpdates.Contains(action)) {
                continuousUpdates.Add(action);
            }
        }

        public static void RequestContinuousLateUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InLateUpdate) {
                // dont alter the main collections while inside the update
                addContinuousActions.Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (!continuousLateUpdates.Contains(action)) {
                continuousLateUpdates.Add(action);
            }
        }

        public static void RequestContinuousFixedUpdate(Action action) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif
            if (instance == null) {
                if (!Create()) return;
            }
            if (InFixedUpdate) {
                // dont alter the main collections while inside the update
                addContinuousActions.Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (!continuousFixedUpdates.Contains(action)) {
                continuousFixedUpdates.Add(action);
            }
        }

        public static void CancelContinuousUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InUpdate) {
                // dont alter the main collections while inside the update
                if (!removeActions.Contains(action)) {
                    removeActions.Add(action);
                }
                return;
            }
            if (continuousUpdates.Contains(action)) {
                continuousUpdates.Remove(action);
            }
        }

        public static void CancelContinuousLateUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InLateUpdate) {
                // dont alter the main collections while inside the update
                if (!removeActions.Contains(action)) {
                    removeActions.Add(action);
                }
                return;
            }
            if (continuousLateUpdates.Contains(action)) {
                continuousLateUpdates.Remove(action);
            }
        }

        public static void CancelContinuousFixedUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (InFixedUpdate) {
                // dont alter the main collections while inside the update
                if (!removeActions.Contains(action)) {
                    removeActions.Add(action);
                }
                return;
            }
            if (continuousFixedUpdates.Contains(action)) {
                continuousFixedUpdates.Remove(action);
            }
        }

        // prevent adding and removing actions from the collections while 
        // iterating them, these lists can accumulate add/remove actions
        // to be processed at the end of each respective update method
        public static bool InUpdate { get; private set; }
        public static bool InLateUpdate { get; private set; }
        public static bool InFixedUpdate { get; private set; }
        private static List<Action> removeActions = new List<Action>();
        private static List<Action> addActions = new List<Action>();
        private static List<Action> addContinuousActions = new List<Action>();

        private void Update() {
            InUpdate = true;
            foreach (Action action in continuousUpdates) {
                action();
            }
            foreach (Action action in updates) {
                action();
            }
            updates.Clear();
            // remove and add actions that were requested 
            // while iterating the actions above
            if (removeActions.Count > 0) {
                foreach (Action action in removeActions) {
                    continuousUpdates.Remove(action);
                    updates.Remove(action);
                }
                removeActions.Clear();
            }
            if (addActions.Count > 0) {
                foreach (Action action in addActions) {
                    action();
                }
                addActions.Clear();
            }
            if (addContinuousActions.Count > 0) {
                foreach (Action action in addContinuousActions) {
                    continuousUpdates.Add(action);
                }
                addContinuousActions.Clear();
            }
            InUpdate = false;
        }

        private void LateUpdate() {
            InLateUpdate = true;
            foreach (Action action in continuousLateUpdates) {
                action();
            }
            foreach (Action action in lateUpdates) {
                action();
            }
            lateUpdates.Clear();
            instance.enabled = continuousUpdates.Count > 0 || continuousLateUpdates.Count > 0 || continuousFixedUpdates.Count > 0;
            // remove and add actions that were requested 
            // while iterating the actions above
            if (removeActions.Count > 0) {
                foreach (Action action in removeActions) {
                    continuousLateUpdates.Remove(action);
                    lateUpdates.Remove(action);
                }
                removeActions.Clear();
            }
            if (addActions.Count > 0) {
                foreach (Action action in addActions) {
                    action();
                }
                addActions.Clear();
            }
            if (addContinuousActions.Count > 0) {
                foreach (Action action in addContinuousActions) {
                    continuousLateUpdates.Add(action);
                }
                addContinuousActions.Clear();
            }
            InLateUpdate = false;
        }

        private void FixedUpdate() {
            InFixedUpdate = true;
            foreach (Action action in continuousFixedUpdates) {
                action();
            }
            foreach (Action action in fixedUpdates) {
                action();
            }
            fixedUpdates.Clear();
            instance.enabled = continuousUpdates.Count > 0 || continuousLateUpdates.Count > 0 || continuousFixedUpdates.Count > 0;
            // remove and add actions that were requested 
            // while iterating the actions above
            if (removeActions.Count > 0) {
                foreach (Action action in removeActions) {
                    continuousFixedUpdates.Remove(action);
                    fixedUpdates.Remove(action);
                }
                removeActions.Clear();
            }
            if (addActions.Count > 0) {
                foreach (Action action in addActions) {
                    action();
                }
                addActions.Clear();
            }
            if (addContinuousActions.Count > 0) {
                foreach (Action action in addContinuousActions) {
                    continuousFixedUpdates.Add(action);
                }
                addContinuousActions.Clear();
            }
            InFixedUpdate = false;
        }
    }
}