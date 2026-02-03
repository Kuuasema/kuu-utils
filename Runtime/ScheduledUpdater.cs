using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils
{
    public class ScheduledUpdater : MonoBehaviour
    {
        private static ScheduledUpdater instance;
        private static bool quitting;

        private static bool Create()
        {
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

        private void OnApplicationQuit()
        {
            quitting = true;
        }

        public static void Cleanup()
        {
            updates.Clear();
            lateUpdates.Clear();
            fixedUpdates.Clear();
            continuousUpdates.Clear();
            continuousLateUpdates.Clear();
            continuousFixedUpdates.Clear();
        }

        public static void RunCoroutine(IEnumerator coroutine)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            instance.StartCoroutine(coroutine);
        }

        public static void RequestUpdate(Action action, int index = 0)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            highestIndex = Mathf.Max(highestIndex, index);
            if (InUpdate)
            {
                // dont alter the main collections while inside the update
                if (!addActions.ContainsKey(index) || addActions[index] == null)
                    addActions.Add(index, new List<Action>());
                addActions[index].Add(action);
                return;
            }

            if (!instance.enabled) instance.enabled = true;
            if (!updates.ContainsKey(index) || updates[index] == null) updates.Add(index, new List<Action>());
            if (!updates[index].Contains(action))
            {
                updates[index].Add(action);
            }
        }

        public static void RequestLateUpdate(Action action, int index = 0)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            highestIndexLate = Mathf.Max(highestIndexLate, index);
            if (InLateUpdate)
            {
                // dont alter the main collections while inside the update
                if (!addLateActions.ContainsKey(index) || addLateActions[index] == null)
                    addLateActions.Add(index, new List<Action>());
                addLateActions[index].Add(action);
                return;
            }

            if (!instance.enabled) instance.enabled = true;
            if (!lateUpdates.ContainsKey(index) || lateUpdates[index] == null)
                lateUpdates.Add(index, new List<Action>());
            if (!lateUpdates[index].Contains(action))
            {
                lateUpdates[index].Add(action);
            }
        }

        public static void RequestFixedUpdate(Action action, int index = 0)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            highestIndexFixed = Mathf.Max(highestIndexFixed, index);
            if (InFixedUpdate)
            {
                // dont alter the main collections while inside the update
                if (!addFixedActions.ContainsKey(index) || addFixedActions[index] == null)
                    addFixedActions.Add(index, new List<Action>());
                addFixedActions[index].Add(action);
                return;
            }

            if (!instance.enabled) instance.enabled = true;
            if (!fixedUpdates.ContainsKey(index) || fixedUpdates[index] == null)
                fixedUpdates.Add(index, new List<Action>());
            if (!fixedUpdates[index].Contains(action))
            {
                fixedUpdates[index].Add(action);
            }
        }

        public static void RequestContinuousUpdate(Action action, int index = 0)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            highestIndex = Mathf.Max(highestIndex, index);
            if (InUpdate)
            {
                // dont alter the main collections while inside the update
                if (!addContinuousActions.ContainsKey(index) || addContinuousActions[index] == null)
                    addContinuousActions.Add(index, new List<Action>());
                addContinuousActions[index].Add(action);
                return;
            }

            if (!instance.enabled) instance.enabled = true;
            if (!continuousUpdates.ContainsKey(index) || continuousUpdates[index] == null)
                continuousUpdates.Add(index, new List<Action>());
            if (!continuousUpdates[index].Contains(action))
            {
                continuousUpdates[index].Add(action);
            }
        }

        public static void RequestContinuousLateUpdate(Action action, int index = 0)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            highestIndexLate = Mathf.Max(highestIndexLate, index);
            if (InLateUpdate)
            {
                // dont alter the main collections while inside the update
                if (!addContinuousLateActions.ContainsKey(index) || addContinuousLateActions[index] == null)
                    addContinuousLateActions.Add(index, new List<Action>());
                addContinuousLateActions[index].Add(action);
                return;
            }

            if (!instance.enabled) instance.enabled = true;
            if (!continuousLateUpdates.ContainsKey(index) || continuousLateUpdates[index] == null)
                continuousLateUpdates.Add(index, new List<Action>());
            if (!continuousLateUpdates[index].Contains(action))
            {
                continuousLateUpdates[index].Add(action);
            }
        }

        public static void RequestContinuousFixedUpdate(Action action, int index = 0)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if (instance == null)
            {
                if (!Create()) return;
            }

            highestIndexFixed = Mathf.Max(highestIndexFixed, index);
            if (InFixedUpdate)
            {
                // dont alter the main collections while inside the update
                if (!addContinuousFixedActions.ContainsKey(index) || addContinuousFixedActions[index] == null)
                    addContinuousFixedActions.Add(index, new List<Action>());
                addContinuousFixedActions[index].Add(action);
                return;
            }

            if (!instance.enabled) instance.enabled = true;
            if (!continuousFixedUpdates.ContainsKey(index) || continuousFixedUpdates[index] == null)
                continuousFixedUpdates.Add(index, new List<Action>());
            if (!continuousFixedUpdates[index].Contains(action))
            {
                continuousFixedUpdates[index].Add(action);
            }
        }

        public static void CancelContinuousUpdate(Action action)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            if (!InUpdate)
            {
                foreach (List<Action> list in continuousUpdates.Values)
                {
                    if (list.Contains(action)) list.Remove(action);
                }
            }
            else if (!removeActions.Contains(action))
            {
                removeActions.Add(action);
            }
        }

        public static void CancelContinuousLateUpdate(Action action)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            if (!InLateUpdate)
            {
                foreach (List<Action> list in continuousLateUpdates.Values)
                {
                    if (list.Contains(action)) list.Remove(action);
                }
            }
            else if (!removeLateActions.Contains(action))
            {
                removeLateActions.Add(action);
            }
        }

        public static void CancelContinuousFixedUpdate(Action action)
        {
            if (instance == null)
            {
                if (!Create()) return;
            }

            if (!InFixedUpdate)
            {
                foreach (List<Action> list in continuousFixedUpdates.Values)
                {
                    if (list.Contains(action)) list.Remove(action);
                }
            }
            else if (!removeFixedActions.Contains(action))
            {
                removeFixedActions.Add(action);
            }
        }

        // prevent adding and removing actions from the collections while 
        // iterating them, these lists can accumulate add/remove actions
        // to be processed at the start of each respective update method
        public static bool InUpdate { get; private set; }
        public static bool InLateUpdate { get; private set; }
        public static bool InFixedUpdate { get; private set; }
        private static List<Action> removeActions = new List<Action>();
        private static List<Action> removeLateActions = new List<Action>();
        private static List<Action> removeFixedActions = new List<Action>();
        private static Dictionary<int, List<Action>> addActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addLateActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addFixedActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addContinuousActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addContinuousLateActions = new Dictionary<int, List<Action>>();
        private static Dictionary<int, List<Action>> addContinuousFixedActions = new Dictionary<int, List<Action>>();

        private void Update()
        {
            InUpdate = true;
            // remove and add actions that were requested 
            // while iterating the actions above
            if (removeActions.Count > 0)
            {
                int index = -1;
                for (int i = 0; i <= highestIndex; i++)
                {
                    foreach (Action action in removeActions)
                    {
                        if (continuousUpdates.ContainsKey(i))
                        {
                            continuousUpdates[i].Remove(action);
                            if (continuousUpdates[i].Count > 0) index = i;
                            else continuousUpdates.Remove(i);
                        }

                        if (updates.ContainsKey(i))
                        {
                            updates[i].Remove(action);
                            if (updates[i].Count > 0) index = i;
                            else updates.Remove(i);
                        }
                    }
                }

                highestIndex = index;
                removeActions.Clear();
            }

            if (addActions.Count > 0)
            {
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
                if (continuousUpdates.TryGetValue(i, out var continuousUpdate))
                {
                    foreach (Action action in continuousUpdate)
                    {
                        action?.Invoke();
                    }
                }

                if (updates.TryGetValue(i, out var update))
                {
                    foreach (Action action in update)
                    {
                        action?.Invoke();
                    }
                }
            }

            updates.Clear();
            InUpdate = false;
        }

        private void LateUpdate()
        {
            InLateUpdate = true;
            if (removeLateActions.Count > 0)
            {
                int index = -1;
                for (int i = 0; i <= highestIndex; i++)
                {
                    foreach (Action action in removeLateActions)
                    {
                        if (continuousLateUpdates.ContainsKey(i))
                        {
                            continuousLateUpdates[i].Remove(action);
                            if (continuousLateUpdates[i].Count > 0) index = i;
                            else continuousLateUpdates.Remove(i);
                        }

                        if (lateUpdates.ContainsKey(i))
                        {
                            lateUpdates[i].Remove(action);
                            if (lateUpdates[i].Count > 0) index = i;
                            else lateUpdates.Remove(i);
                        }
                    }
                }

                highestIndexLate = index;
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
                if (continuousLateUpdates.TryGetValue(i, out var lateUpdate))
                {
                    foreach (Action action in lateUpdate)
                    {
                        action?.Invoke();
                    }
                }

                if (lateUpdates.TryGetValue(i, out var update))
                {
                    foreach (Action action in update)
                    {
                        action?.Invoke();
                    }
                }
            }

            lateUpdates.Clear();
            InLateUpdate = false;
        }

        private void FixedUpdate()
        {
            InFixedUpdate = true;
            if (removeFixedActions.Count > 0)
            {
                int index = -1;
                for (int i = 0; i <= highestIndex; i++)
                {
                    foreach (Action action in removeFixedActions)
                    {
                        if (continuousFixedUpdates.ContainsKey(i))
                        {
                            continuousFixedUpdates[i].Remove(action);
                            if (continuousFixedUpdates[i].Count > 0) index = i;
                            else continuousFixedUpdates.Remove(i);
                        }

                        if (fixedUpdates.ContainsKey(i))
                        {
                            fixedUpdates[i].Remove(action);
                            if (fixedUpdates[i].Count > 0) index = i;
                            else fixedUpdates.Remove(i);
                        }
                    }
                }

                highestIndexFixed = index;
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
                if (continuousFixedUpdates.TryGetValue(i, out var fixedUpdate
                    ))
                {
                    foreach (Action action in fixedUpdate)
                    {
                        action?.Invoke();
                    }
                }

                if (fixedUpdates.TryGetValue(i, out var update))
                {
                    foreach (Action action in update)
                    {
                        action?.Invoke();
                    }
                }
            }

            fixedUpdates.Clear();
            InFixedUpdate = false;
        }
    }
}