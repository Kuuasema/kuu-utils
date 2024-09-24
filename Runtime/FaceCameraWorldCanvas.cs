using UnityEngine;


namespace Kuuasema.Utils {
    [RequireComponent(typeof(Canvas))]
    public class FaceCameraWorldCanvas : MonoBehaviour {
        public bool FixedUp;
        private Canvas worldCanvas;
        private Vector3 startPosition;
        private void Start() {
            this.worldCanvas = this.GetComponent<Canvas>();
            if (this.worldCanvas.worldCamera == null) {
                this.worldCanvas.worldCamera = MainCamera.Instance.Behaviour;
            }
            this.startPosition = this.transform.localPosition;
        }

        private void Update() {
            this.transform.localPosition = Quaternion.Inverse(this.transform.parent.localRotation) * this.startPosition;
            this.transform.LookAt(this.worldCanvas.worldCamera.transform);
            this.transform.rotation *= Quaternion.Euler(0, 180, 0);
            if (this.FixedUp) {
                this.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(this.transform.forward.normalized, Vector3.up));
            }
        }
    }
}