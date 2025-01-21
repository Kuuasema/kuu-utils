using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    /**
    * A generic component that keeps track of said comonents, allowing only one to be active at a given time.
    */
    public class SingleBehaviour<T> : MonoBehaviour where T : Behaviour {
        private static List<SingleBehaviour<T>> instances = new List<SingleBehaviour<T>>();
        public static SingleBehaviour<T> Instance => instances.Count == 0 ? null : instances[instances.Count - 1];

        [Header("Single Behaviour")]
        [SerializeField]
        private T behaviour;
        public T Behaviour => this.behaviour;
        [SerializeField]
        private bool dontDestroy;
        [SerializeField]
        private bool disableGameObject;
        private bool isActivated;
        protected virtual void Awake() {
            if (this.dontDestroy) {
                this.transform.SetParent(null);
                DontDestroyOnLoad(this.gameObject);
            }
            if (this.behaviour == null) {
                this.behaviour = this.GetComponent<T>();
            }
            // this.Activate();
            // instances.Add(this);
        }

        protected virtual void OnDestroy() {
            // instances.Remove(this);
        }

        protected virtual void OnEnable() {
            if (this.isActivated) return;
            if (instances.Count > 0) {
                if (this.dontDestroy) {
                    // fallback component from "Main" scene loaded additively, deactivate and insert on bottom
                    this.OnDeactivate();
                    instances.Insert(0, this);
                    return;
                }
                if (instances[^1] == this) {
                    this.OnActivate();
                    return;
                }
                instances[^1].OnDeactivate();
            }
            instances.Add(this);
            this.OnActivate();
        }
        
        private void OnDisable() {
            if (!this.isActivated) return;
            this.OnDeactivate();
            if (instances.Count > 1) {
                instances.Remove(this);
            }
            if (instances.Count > 0 && !RuntimeUtils.IsQuitting) {
                instances[^1].OnActivate();
            }
        }

        protected virtual void OnActivate() {
            this.isActivated = true;
            if (this.disableGameObject) {
                this.gameObject.TrySetActive(true);
            }
            this.behaviour.enabled = true;
        }

        protected virtual void OnDeactivate() {
            this.isActivated = false;
            this.behaviour.enabled = false;
            if (this.disableGameObject) {
                this.gameObject.TrySetActive(false);
            }
        }
    }
}