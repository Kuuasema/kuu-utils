using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Kuuasema.Utils
{
    public class InputRaycaster : MonoBehaviour
    {
        [SerializeField] internal LayerMask commonHitLayers;
        [SerializeField] internal LayerMask blockingLayers;
        [SerializeField] internal LayerMask hudBlockerLayer;

        public static bool IsOverHUD = false;

        public static ScreenRaycasts Cursor { get; private set; }
        public static ScreenRaycasts ScreenCenter { get; private set; }
        public static int COMMON_LAYERS { get; private set; }
        public static int BLOCKING_LAYERS { get; private set; }
        public static int HUD_BLOCKER_LAYER { get; private set; }

        private ScreenRaycasts cursor;
        private ScreenRaycasts screenCenter;

        private static List<ScreenRaycasts> AllRaycasts = new List<ScreenRaycasts>()
        {
            Cursor,
            ScreenCenter
        };

        protected void Awake()
        {
            cursor = new ScreenRaycasts(this);
            screenCenter = new ScreenRaycasts(this);
            Cursor = this.cursor;
            ScreenCenter = this.screenCenter;
            COMMON_LAYERS = this.commonHitLayers;
            BLOCKING_LAYERS = this.blockingLayers;
            HUD_BLOCKER_LAYER = this.hudBlockerLayer;
        }

        private void Update()
        {
            // setup positions
            cursor.ScreenPosition = UnityEngine.Input.mousePosition;
            screenCenter.ScreenPosition = new Vector3(Screen.width, Screen.height, 0) / 2;

            // Clear and update raycasts
            cursor.Clear();
            screenCenter.Clear();

            // Do extra HUD check for cursor raycasts
            cursor.Raycast(true);
            screenCenter.Raycast(false);
        }

        private void OnDisable()
        {
            if (AllRaycasts != null)
            {
                foreach (ScreenRaycasts raycasts in AllRaycasts)
                {
                    if (raycasts != null) raycasts.Clear();
                }
            }
        }

        ///////
        //// ScreenRaycasts
        ///////

        public class ScreenRaycasts
        {
            const int MAX_RAYCAST_HITS = 64;

            public Vector2 ScreenPosition { get; set; }
            public Ray Ray { get; private set; }
            public RaycastHit[] HitsAll { get; set; } = new RaycastHit[MAX_RAYCAST_HITS];
            public RaycastHit[] HitsHUD { get; set; } = new RaycastHit[MAX_RAYCAST_HITS];
            public List<RaycastHit> HitsSorted { get; set; } = new List<RaycastHit>();
            public Vector3 ZeroPlaneHit { get; private set; }

            private InputRaycaster raycaster { get; set; }

            public ScreenRaycasts(InputRaycaster raycaster)
            {
                this.raycaster = raycaster;
            }

            public void Clear()
            {
                this.HitsSorted.Clear();
            }

            public void Raycast(bool doHUDCheck)
            {
                if (Camera.main != null)
                {
                    this.Ray = GameCamera.Camera.ScreenPointToRay(this.ScreenPosition);
                    int hits = Physics.RaycastNonAlloc(this.Ray, this.HitsAll);
                    for (int i = 0; i < hits; i++)
                    {
                        this.HitsSorted.Add(this.HitsAll[i]);
                    }

                    this.HitsSorted.Sort((a, b) => a.distance.CompareTo(b.distance));
                }
                else if (MainCamera.Instance != null)
                {
                    this.Ray = MainCamera.Camera.ScreenPointToRay(this.ScreenPosition);
                    int hits = Physics.RaycastNonAlloc(this.Ray, this.HitsAll);
                    for (int i = 0; i < hits; i++)
                    {
                        this.HitsSorted.Add(this.HitsAll[i]);
                    }

                    this.HitsSorted.Sort((a, b) => a.distance.CompareTo(b.distance));
                }

                if (doHUDCheck && HUDCamera.Instance != null)
                {
                    bool isOverHUDThisFrame = false;
                    this.Ray = HUDCamera.Camera.ScreenPointToRay(this.ScreenPosition);
                    int hitsHUD = Physics.RaycastNonAlloc(this.Ray, this.HitsHUD);

                    for (int i = 0; !isOverHUDThisFrame && i < hitsHUD; i++)
                    {
                        int hitLayer = 1 << HitsHUD[i].collider.gameObject.layer;
                        isOverHUDThisFrame = (this.raycaster.hudBlockerLayer & hitLayer) != 0;
                    }

                    InputRaycaster.IsOverHUD = isOverHUDThisFrame;
                }

                if (new Plane(Vector3.up, Vector3.zero).Raycast(this.Ray, out float enter))
                {
                    this.ZeroPlaneHit = this.Ray.GetPoint(enter);
                }
            }

            public Component GetHit(out RaycastHit? hitInfo, int layerMask = int.MaxValue,
                IEnumerable<Type> componentTypes = null, List<Component> filter = null)
            {
                if (TryGetHit(out Component hitComponent, out hitInfo, layerMask, componentTypes, filter))
                {
                    return hitComponent;
                }

                return null;
            }

            public bool TryGetHit(out RaycastHit? hitInfo, int layerMask = int.MaxValue)
            {
                hitInfo = null;
                foreach (RaycastHit hit in this.HitsSorted)
                {
                    if (hit.collider == null || hit.collider.gameObject == null) continue;
                    int hitLayer = 1 << hit.collider.gameObject.layer;
                    if ((layerMask & hitLayer) == hitLayer)
                    {
                        hitInfo = hit;
                        return true;
                    }
                }

                return false;
            }

            public bool TryGetHit(out Component hitComponent, out RaycastHit? hitInfo, IEnumerable<Type> componentTypes,
                List<Component> filter)
            {
                return TryGetHit(out hitComponent, out hitInfo, int.MaxValue, componentTypes, filter);
            }

            public bool TryGetHit(out Component hitComponent, out RaycastHit? hitInfo, int layerMask = int.MaxValue,
                IEnumerable<Type> componentTypes = null, List<Component> filter = null)
            {
                hitInfo = null;
                hitComponent = null;
                foreach (RaycastHit hit in this.HitsSorted)
                {
                    if (hit.collider == null || hit.collider.gameObject == null) continue;
                    GameObject hitObject = hit.collider.gameObject;
                    int hitLayer = 1 << hitObject.layer;
                    if ((layerMask & hitLayer) == hitLayer)
                    {
                        if (componentTypes != null)
                        {
                            int closestDistance = int.MaxValue;
                            Component closestComponent = null;
                            foreach (Type componentType in componentTypes)
                            {
                                Component behaviour = hitObject.GetComponentInParent(componentType);
                                if (filter != null && filter.Count > 0 && !filter.Contains(behaviour))
                                {
                                    // not registered for event
                                }
                                else
                                {
                                    if (hitObject.transform.TryCountParentDistance(behaviour.transform,
                                            out int distance))
                                    {
                                        if (distance < closestDistance)
                                        {
                                            closestDistance = distance;
                                            closestComponent = behaviour;
                                        }
                                    }
                                }
                            }

                            if (closestComponent != null)
                            {
                                hitInfo = hit;
                                hitComponent = closestComponent;
                                return true;
                            }
                        }

                        if ((this.raycaster.blockingLayers & hitLayer) == hitLayer)
                        {
                            return false;
                        }
                    }
                }

                return false;
            }

            public bool IsOverHUD()
            {
                return this.HitsSorted.Select(hit => hit.collider.gameObject.layer)
                    .Select(hitLayer => (this.raycaster.hudBlockerLayer & hitLayer) == hitLayer).FirstOrDefault();
            }

            public T GetHit<T>(int layerMask = int.MaxValue) where T : Component
            {
                if (TryGetHit<T>(out T behaviour, out RaycastHit? hitInfo, layerMask, null))
                {
                    return behaviour;
                }

                return null;
            }

            public bool TryGetHit<T>(out T behaviour, out RaycastHit? hitInfo, int layerMask = int.MaxValue,
                List<GameObject> filter = null) where T : Component
            {
                behaviour = null;
                if (TryGetHit(out Component _behaviour, typeof(T), out hitInfo, layerMask, filter))
                {
                    behaviour = _behaviour as T;
                    return behaviour != null;
                }

                return false;
            }

            public bool TryGetHit(out Component behaviour, System.Type componentType, out RaycastHit? hitInfo,
                int layerMask = int.MaxValue, List<GameObject> filter = null)
            {
                behaviour = null;
                hitInfo = null;
                foreach (RaycastHit hit in this.HitsSorted)
                {
                    // since raycasts are collected only once each frame
                    // and an object might be destroyed later that frame
                    // null check is needed because the list may be iterated after the destruction
                    if (hit.collider == null || hit.collider.gameObject == null) continue;
                    int hitLayer = 1 << hit.collider.gameObject.layer;
                    if ((layerMask & hitLayer) == hitLayer)
                    {
                        hitInfo = hit;
                        behaviour = hit.collider.gameObject.GetComponent(componentType);
                        if (behaviour == null)
                        {
                            behaviour = hit.collider.gameObject.GetComponentInParent(componentType);
                        }

                        if (behaviour != null)
                        {
                            if (filter != null && filter.Count > 0 && !filter.Contains(behaviour.gameObject))
                            {
                                behaviour = null;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        if ((this.raycaster.blockingLayers & hitLayer) == hitLayer)
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
        }
    }
}