using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    public class RecycleToQueue : MonoBehaviour {
        public Queue<GameObject> Queue { get; set; }
        public void Recycle() {
            if (this.Queue != null) {
                this.OnRecycle();
                this.gameObject.SetActive(false);
                this.Queue.Enqueue(this.gameObject);
            }
        }

        protected virtual void OnRecycle() {

        }
    }
}