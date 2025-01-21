using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    /**
     * Ensures only one camera is active on the scene for rendering HUD, use together with GameCamera.
     */
    public class HUDCamera : SingleBehaviour<Camera> {
        public static Camera Camera => HUDCamera.Instance != null ? HUDCamera.Instance.Behaviour : Camera.main;
    }
}
