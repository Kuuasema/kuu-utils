using UnityEngine;
namespace Kuuasema.Utils {
    /**
     * Ensures only one camera is active on the scene if they are accompanied by this component.
     * Use GameCamera and HUDCamera instead if needed.
     */
    public class MainCamera : SingleBehaviour<Camera> {
        public static Camera Camera => MainCamera.Instance != null ? MainCamera.Instance.Behaviour : Camera.main;
    }
}