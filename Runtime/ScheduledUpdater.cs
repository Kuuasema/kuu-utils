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

        private static Dictionary<int, List<Action>> updates = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> lateUpdates = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> fixedUpdates = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> continuousUpdates = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> continuousLateUpdates = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> continuousFixedUpdates = new Dictionary<int, List<Action>>();
        private static int highestIndex = -1;
        private static int highestIndexLate = -1;
        private static int highestIndexFixed = -1;

        private void OnApplicationQuit() {
            quitting = true;
        }

        public static void RunCoroutine(IEnumerator coroutine) {
            if (instance == null) {
                if (!Create()) return;
            }
            instance.StartCoroutine(coroutine);
        }

        public static void RequestUpdate(Action action, int index = 0) {
            if (instance == null) {
                if (!Create()) return;
            }
            highestIndex = Mathf.Max(highestIndex, index);
            if (InUpdate) {
                // dont alter the main collections while inside the update
                if (addActions[index] == null) addActions.Add(index, new List<Action>());
                addActions[index].Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (updates[index] == null) updates.Add(index, new List<Action>());
            if (!updates[index].Contains(action)) {
                updates[index].Add(action);
            }
        }

        public static void RequestLateUpdate(Action action, int index = 0) {
            if (instance == null) {
                if (!Create()) return;
            }
            highestIndexLate = Mathf.Max(highestIndexLate, index);
            if (InLateUpdate) {
                // dont alter the main collections while inside the update
                if (addLateActions[index] == null) addLateActions.Add(index, new List<Action>());
                addLateActions[index].Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (lateUpdates[index] == null) lateUpdates.Add(index, new List<Action>());
            if (!lateUpdates[index].Contains(action)) {
                lateUpdates[index].Add(action);
            }
        }

        public static void RequestFixedUpdate(Action action, int index = 0) {
            if (instance == null) {
                if (!Create()) return;
            }
            highestIndexFixed = Mathf.Max(highestIndexFixed, index);
            if (InFixedUpdate) {
                // dont alter the main collections while inside the update
                if (addFixedActions[index] == null) addFixedActions.Add(index, new List<Action>());
                addFixedActions[index].Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (fixedUpdates[index] == null) fixedUpdates.Add(index, new List<Action>());
            if (!fixedUpdates[index].Contains(action)) {
                fixedUpdates[index].Add(action);
            }
        }

        public static void RequestContinuousUpdate(Action action, int index = 0) {
            if (instance == null) {
                if (!Create()) return;
            }
            highestIndex = Mathf.Max(highestIndex, index);
            if (InUpdate) {
                // dont alter the main collections while inside the update
                if (addContinuousActions[index] == null) addContinuousActions.Add(index, new List<Action>());
                addContinuousActions[index].Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (continuousUpdates[index] == null) continuousUpdates.Add(index, new List<Action>());
            if (!continuousUpdates[index].Contains(action)) {
                continuousUpdates[index].Add(action);
            }
        }

        public static void RequestContinuousLateUpdate(Action action, int index = 0) {
            if (instance == null) {
                if (!Create()) return;
            }
            highestIndexLate = Mathf.Max(highestIndexLate, index);
            if (InLateUpdate) {
                // dont alter the main collections while inside the update
                if (addContinuousLateActions[index] == null) addContinuousLateActions.Add(index, new List<Action>());
                addContinuousLateActions[index].Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (continuousLateUpdates[index] == null) continuousLateUpdates.Add(index, new List<Action>());
            if (!continuousLateUpdates[index].Contains(action)) {
                continuousLateUpdates[index].Add(action);
            }
        }

        public static void RequestContinuousFixedUpdate(Action action, int index = 0) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif
            if (instance == null) {
                if (!Create()) return;
            }
            highestIndexFixed = Mathf.Max(highestIndexFixed, index);
            if (InFixedUpdate) {
                // dont alter the main collections while inside the update
                if (addContinuousFixedActions[index] == null) addContinuousFixedActions.Add(index, new List<Action>());
                addContinuousFixedActions[index].Add(action);
                return;
            }
            if (!instance.enabled) instance.enabled = true;
            if (continuousFixedUpdates[index] == null) continuousFixedUpdates.Add(index, new List<Action>());
            if (!continuousFixedUpdates[index].Contains(action)) {
                continuousFixedUpdates[index].Add(action);
            }
        }

        public static void CancelContinuousUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (!removeActions.Contains(action))
            {
                removeActions.Add(action);
            }
        }

        public static void CancelContinuousLateUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (!removeLateActions.Contains(action))
            {
                removeLateActions.Add(action);
            }
        }

        public static void CancelContinuousFixedUpdate(Action action) {
            if (instance == null) {
                if (!Create()) return;
            }
            if (!removeFixedActions.Contains(action))
            {
                removeFixedActions.Add(action);
            }
        }

        // prevent adding and removing actions from the collections while 
        // iterating them, these lists can accumulate add/remove actions
        // to be processed at the start of each respective update method
        public static bool InUpdate { get; private set; }
        public static bool InLateUpdate { get; private set; }
        public static bool InFixedUpdate { get; private set; }
        private static List<Action> removeActions = new List<Action>();
        private static List<Action> removeLateActions = new List<Action>();
        private static List<Action> removeFixedActions = new List<Action>();
        private static Dictionary<int, List<Action>> addActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addLateActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addFixedActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addContinuousActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addContinuousLateActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addContinuousFixedActions = new Dictionary<int, List<Action>>();

        private void Update() {
            InUpdate = true;
            // remove and add actions that were requested 
            // while iterating the actions above
            if (removeActions.Count > 0) {
                foreach (Action action in removeActions)
                {
                    int index = -1;
                    for (int i = 0; i <= highestIndex; i++)
                    {
                        if (continuousUpdates[i] != null && continuousUpdates[i].Count > 0)
                        {
                            continuousUpdates[i].Remove(action);
                            if (continuousUpdates[i].Count > 0) index = i;
                        }
                        if (updates[i] != null && updates[i].Count > 0)
                        {
                            updates[i].Remove(action);
                            if (updates[i].Count > 0) index = i;
                        }
                    }
                    highestIndex = index;
                }
                removeActions.Clear();
            }
            if (addActions.Count > 0) {
                foreach (KeyValuePair<int, List<Action>> kvp in addActions)
                {
                    highestIndex = Mathf.Max(highestIndex, kvp.Key);
                    if (updates.ContainsKey(kvp.Key))
                    {
                        updates[kvp.Key].AddRange(kvp.Value);
                    }
                    else updates.Add(kvp.Key, kvp.Value);
                }
                addActions.Clear();
            }
            if (addContinuousActions.Count > 0)
            {
                foreach (KeyValuePair<int, List<Action>> kvp in addContinuousActions)
                {
                    highestIndex = Mathf.Max(highestIndex, kvp.Key);
                    if (continuousUpdates.ContainsKey(kvp.Key))
                    {
                        continuousUpdates[kvp.Key].AddRange(kvp.Value);
                    }
                    else continuousUpdates.Add(kvp.Key, kvp.Value);
                }
                addContinuousActions.Clear();
            }
            for (int i = 0; i <= highestIndex; i++)
            {
                if (continuousUpdates[i] != null)
                {
                    foreach (Action action in continuousUpdates[i])
                    {
                        action();
                    }
                }
                if (updates[i] != null)
                {
                    foreach (Action action in updates[i])
                    {
                        action();
                    }
                }
            }
            updates.Clear();
            InUpdate = false;
        }

        private void LateUpdate() {
            InLateUpdate = true;
            if (removeActions.Count > 0)
            {
                foreach (Action action in removeActions)
                {
                    int index = -1;
                    for (int i = 0; i <= highestIndexLate; i++)
                    {
                        if (continuousLateUpdates[i] != null && continuousLateUpdates[i].Count > 0)
                        {
                            continuousLateUpdates[i].Remove(action);
                            if (continuousLateUpdates[i].Count > 0) index = i;
                        }
                        if (lateUpdates[i] != null && lateUpdates[i].Count > 0)
                        {
                            lateUpdates[i].Remove(action);
                            if (lateUpdates[i].Count > 0) index = i;
                        }
                    }
                    highestIndexLate = index;
                }
                removeActions.Clear();
            }
            if (addLateActions.Count > 0)
            {
                foreach (KeyValuePair<int, List<Action>> kvp in addLateActions)
                {
                    highestIndexLate = Mathf.Max(highestIndexLate, kvp.Key);
                    if (lateUpdates.ContainsKey(kvp.Key))
                    {
                        lateUpdates[kvp.Key].AddRange(kvp.Value);
                    }
                    else lateUpdates.Add(kvp.Key, kvp.Value);
                }
                addLateActions.Clear();
            }
            if (addContinuousLateActions.Count > 0)
            {
                foreach (KeyValuePair<int, List<Action>> kvp in addContinuousLateActions)
                {
                    highestIndexLate = Mathf.Max(highestIndexLate, kvp.Key);
                    if (continuousLateUpdates.ContainsKey(kvp.Key))
                    {
                        continuousLateUpdates[kvp.Key].AddRange(kvp.Value);
                    }
                    else continuousLateUpdates.Add(kvp.Key, kvp.Value);
                }
                addContinuousLateActions.Clear();
            }
            for (int i = 0; i <= highestIndexLate; i++)
            {
                if (continuousLateUpdates[i] != null)
                {
                    foreach (Action action in continuousLateUpdates[i])
                    {
                        action();
                    }
                }
                if (lateUpdates[i] != null)
                {
                    foreach (Action action in lateUpdates[i])
                    {
                        action();
                    }
                }
            }
            lateUpdates.Clear();
            InLateUpdate = false;
        }

        private void FixedUpdate()
        {
            InFixedUpdate = true;
            if (removeActions.Count > 0)
            {
                foreach (Action action in removeActions)
                {
                    int index = -1;
                    for (int i = 0; i <= highestIndexFixed; i++)
                    {
                        if (continuousFixedUpdates[i] != null && continuousFixedUpdates[i].Count > 0)
                        {
                            continuousFixedUpdates[i].Remove(action);
                            if (continuousFixedUpdates[i].Count > 0) index = i;
                        }
                        if (fixedUpdates[i] != null && fixedUpdates[i].Count > 0)
                        {
                            fixedUpdates[i].Remove(action);
                            if (fixedUpdates[i].Count > 0) index = i;
                        }
                    }
                    highestIndexFixed = index;
                }
                removeActions.Clear();
            }
            if (addFixedActions.Count > 0)
            {
                foreach (KeyValuePair<int, List<Action>> kvp in addFixedActions)
                {
                    highestIndexFixed = Mathf.Max(highestIndexFixed, kvp.Key);
                    if (fixedUpdates.ContainsKey(kvp.Key))
                    {
                        fixedUpdates[kvp.Key].AddRange(kvp.Value);
                    }
                    else fixedUpdates.Add(kvp.Key, kvp.Value);
                }
                addFixedActions.Clear();
            }
            if (addContinuousFixedActions.Count > 0)
            {
                foreach (KeyValuePair<int, List<Action>> kvp in addContinuousFixedActions)
                {
                    highestIndexFixed = Mathf.Max(highestIndexFixed, kvp.Key);
                    if (continuousFixedUpdates.ContainsKey(kvp.Key))
                    {
                        continuousFixedUpdates[kvp.Key].AddRange(kvp.Value);
                    }
                    else continuousFixedUpdates.Add(kvp.Key, kvp.Value);
                }
                addContinuousFixedActions.Clear();
            }
            for (int i = 0; i <= highestIndexFixed; i++)
            {
                if (continuousFixedUpdates[i] != null)
                {
                    foreach (Action action in continuousFixedUpdates[i])
                    {
                        action();
                    }
                }
                if (fixedUpdates[i] != null)
                {
                    foreach (Action action in fixedUpdates[i])
                    {
                        action();
                    }
                }
            }
            fixedUpdates.Clear();
            InFixedUpdate = false;
        }
    }
}