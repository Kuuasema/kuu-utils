using UnityEngine;

namespace Kuuasema.Utils
{
    /**
     * Ensures only one camera is active on the scene for rendering game, use together with HUDCamera.
     */
    public class GameCamera : MonoBehaviour
    {
        private static GameCamera instance = null;

        public static GameCamera Instance => instance;

        public static Camera Camera => instance?.GameCam;


        private Camera _gameCam;

        private Camera GameCam
        {
            get
            {
                if (_gameCam != null || TryGetComponent(out _gameCam)) return _gameCam;

                return Camera.main;
            }
        }

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }
    }
}