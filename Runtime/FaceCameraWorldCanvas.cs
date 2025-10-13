using UnityEngine;


namespace Kuuasema.Utils {
    [RequireComponent(typeof(Canvas))]
    public class FaceCameraWorldCanvas : MonoBehaviour {
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
            this.transform.forward = this.worldCanvas.worldCamera.transform.forward;
        }
    }
}