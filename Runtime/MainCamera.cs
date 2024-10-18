using UnityEngine;
namespace Kuuasema.Utils {
    // ensures only one camera is active on the scene if they are accompanied by this component
    public class MainCamera : SingleBehaviour<Camera> {
        public static Camera Camera => MainCamera.Instance != null ? MainCamera.Instance.Behaviour : Camera.main;
    }
}