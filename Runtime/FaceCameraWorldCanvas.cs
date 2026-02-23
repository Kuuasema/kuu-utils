using UnityEngine;


namespace Kuuasema.Utils
{
    [RequireComponent(typeof(Canvas))]
    public class FaceCameraWorldCanvas : MonoBehaviour
    {
        private Canvas worldCanvas;
        private Vector3 startPosition;

        private void Start()
        {
            worldCanvas = GetComponent<Canvas>();
            if (worldCanvas.worldCamera == null)
            {
                worldCanvas.worldCamera = Camera.main;
            }

            startPosition = transform.localPosition;
        }

        private void Update()
        {
            if (worldCanvas == null) worldCanvas = GetComponent<Canvas>();
            if (worldCanvas.worldCamera == null) worldCanvas.worldCamera = Camera.main;
            transform.localPosition = Quaternion.Inverse(transform.parent.localRotation) * startPosition;
            transform.forward = worldCanvas.worldCamera.transform.forward;
        }
    }
}