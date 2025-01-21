using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    /**
     * Ensures only one camera is active on the scene for rendering game, use together with HUDCamera.
     */
    public class GameCamera : SingleBehaviour<Camera> {
        public static Camera Camera => GameCamera.Instance != null ? GameCamera.Instance.Behaviour : Camera.main;
    }
}
