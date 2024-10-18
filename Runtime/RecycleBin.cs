using UnityEngine;

namespace Kuuasema.Utils {
    public class RecycleBin : MonoBehaviour {

        private int cleanupTreshold = 100;

        private static RecycleBin instance;
        void Awake() {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private static RecycleBin GetInstance() {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<RecycleBin>();
                if (instance == null) {
                    GameObject go = new GameObject("RecycleBin [0]");
                    instance = go.AddComponent<RecycleBin>();
                }
            }
            return instance;
        }
        
        public static void PlaceInBin(GameObject gameObject) {
            RecycleBin instance = GetInstance();
            gameObject.transform.parent = instance.transform;

            instance.name = $"RecycleBin [{instance.transform.childCount}][?]";

            if (instance.transform.childCount > instance.cleanupTreshold) {
                instance.EmptyGarbage();
                if (instance.transform.childCount > instance.cleanupTreshold) {
                    instance.cleanupTreshold *= 2;
                }
            }
        }

        [ContextMenu("Empty Bin")]
        private void EmptyBin() {
            for (int i = this.transform.childCount - 1; i >= 0; i--) {
                GameObject child = this.transform.GetChild(i).gameObject;
                if (child != null) {
                    if (Application.isPlaying) {
                        Destroy(child);
                    } else {
                        DestroyImmediate(child);
                    }
                }
            } 
            this.name = $"RecycleBin [0][0]";
        }

        [ContextMenu("Empty Garbage")]
        private void EmptyGarbage() {
            for (int i = this.transform.childCount - 1; i >= 0; i--) {
                GameObject child = this.transform.GetChild(i).gameObject;
                if (child != null) {
                    if (child.TryGetComponent<RecycleToQueue>(out RecycleToQueue recycle)) {
                        if (recycle.Queue != null) {
                            continue;
                        }
                    }
                    if (Application.isPlaying) {
                        Destroy(child);
                    } else {
                        DestroyImmediate(child);
                    }
                }
            } 
            this.name = $"RecycleBin [{this.transform.childCount}][0]";
        }

        [ContextMenu("Log Contents")]
        private void LogContents() {
            int pooled = 0;
            int garbage = 0;
            for (int i = this.transform.childCount - 1; i >= 0; i--) {
                GameObject child = this.transform.GetChild(i).gameObject;
                if (child.TryGetComponent<RecycleToQueue>(out RecycleToQueue recycle)) {
                    if (recycle.Queue != null) {
                        pooled++;
                        continue;
                    }
                }
                garbage++;
            } 
            Debug.Log($"RecycleBin: Pooled = {pooled}, Garbage = {garbage}");
            instance.name = $"RecycleBin [{pooled}][{garbage}]";
        }
    }
}