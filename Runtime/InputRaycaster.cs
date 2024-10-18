using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kuuasema.Utils {
    public class InputRaycaster : MonoBehaviour { // EnableViewModel

        public static ScreenRaycasts Cursor { get; private set; } = new ScreenRaycasts();
        public static ScreenRaycasts ScreenCenter { get; private set; } = new ScreenRaycasts();
        
        // private static RaycastHit[] cursorHitsAll = new RaycastHit[MAX_RAYCAST_HITS];
        // private static List<RaycastHit> cursorHitsSorted = new List<RaycastHit>();

        private static List<ScreenRaycasts> AllRaycasts = new List<ScreenRaycasts>() {
            Cursor,
            ScreenCenter
        };

        private void Update() {
            // setup positions
            Cursor.ScreenPosition = UnityEngine.Input.mousePosition;
            ScreenCenter.ScreenPosition = new Vector3(Screen.width, Screen.height, 0) / 2;
            // raycast
            foreach (ScreenRaycasts raycasts in AllRaycasts) {
                raycasts.Clear();
                raycasts.Raycast();
            }
            // // 1.) get full raycast under cursor, then re-use that result for everything else
            // cursorHitsSorted.Clear();
            // Ray cursorRay = MainCamera.Instance.Behaviour.ScreenPointToRay(Input.mousePosition);
            // int hits = Physics.RaycastNonAlloc(cursorRay, cursorHitsAll);
            // for (int i = 0; i < hits; i++) {
            //     cursorHitsSorted.Add(cursorHitsAll[i]);
            // }
            // cursorHitsSorted.Sort((a,b) => a.distance.CompareTo(b.distance));
        }

        private void OnDisable() {
            // cursorHitsSorted.Clear();
            foreach (ScreenRaycasts raycasts in AllRaycasts) {
                raycasts.Clear();
            }
        }

        // protected override void OnEnableView() {
        //     this.enabled = true;
        // }
        // protected override void OnDisableView() {
        //     this.enabled = false;
        // }

        // public static Component GetCursorHit(out RaycastHit? hitInfo, IEnumerable<Type> componentTypes = null, List<Component> filter = null, int layerMask = int.MaxValue) {
        //     if (TryGetCursorHit(out Component hitComponent, out hitInfo, componentTypes, filter, layerMask)) {
        //         return hitComponent;
        //     }
        //     return null;
        // }

        // public static bool TryGetCursorHit(out RaycastHit? hitInfo, int layerMask = int.MaxValue) {
        //     hitInfo = null;
        //     foreach (RaycastHit hit in cursorHitsSorted) {
        //         int hitLayer = 1 << hit.collider.gameObject.layer;
        //         if ((layerMask & hitLayer) == hitLayer) {
        //             hitInfo = hit;
        //             return true;
        //         }
        //     }
        //     return false;
        // }

        // public static bool TryGetCursorHit(out Component hitComponent, out RaycastHit? hitInfo, IEnumerable<Type> componentTypes = null, List<Component> filter = null, int layerMask = int.MaxValue) {
        //     hitInfo = null;
        //     hitComponent = null;
        //     foreach (RaycastHit hit in cursorHitsSorted) {
        //         GameObject hitObject = hit.collider.gameObject;
        //         int hitLayer = 1 << hitObject.layer;
        //         if ((layerMask & hitLayer) == hitLayer) {
                    
        //             if (componentTypes != null) {
        //                 int closestDistance = int.MaxValue;
        //                 Component closestComponent = null;
        //                 foreach (Type componentType in componentTypes) {
        //                     Component behaviour = hitObject.GetComponentInParent(componentType);
        //                     if (filter != null && filter.Count > 0 && !filter.Contains(behaviour)) {
        //                         // not registered for event
        //                     } else {
        //                         if (hitObject.transform.TryCountParentDistance(behaviour.transform, out int distance)) {
        //                             if (distance < closestDistance) {
        //                                 closestDistance = distance;
        //                                 closestComponent = behaviour;
        //                             }
        //                         }
        //                     }
        //                 }

        //                 if (closestComponent != null) {
        //                     hitInfo = hit;
        //                     hitComponent = closestComponent;
        //                     return true;
        //                 }
        //             }
        //         }
        //     }
        //     return false;
        // }

        // public static T GetCursorHit<T>(int layerMask = int.MaxValue) where T : Component {
        //     if (TryGetCursorHit<T>(out T behaviour, out RaycastHit? hitInfo, null, layerMask)) {
        //         return behaviour;
        //     }
        //     return null;
        // }

        // public static bool TryGetCursorHit<T>(out T behaviour, out RaycastHit? hitInfo, List<GameObject> filter = null, int layerMask = int.MaxValue) where T : Component {
        //     behaviour = null;
        //     if (TryGetCursorHit(out Component _behaviour, typeof(T), out hitInfo, filter, layerMask)) {
        //         behaviour = _behaviour as T;
        //         return behaviour != null;
        //     }
        //     return false;
        // }

        // public static bool TryGetCursorHit(out Component behaviour, System.Type componentType, out RaycastHit? hitInfo, List<GameObject> filter = null, int layerMask = int.MaxValue) {
        //     behaviour = null;
        //     hitInfo = null;
        //     foreach (RaycastHit hit in cursorHitsSorted) {
        //         int hitLayer = 1 << hit.collider.gameObject.layer;
        //         if ((layerMask & hitLayer) == hitLayer) {
        //             hitInfo = hit;
        //             behaviour = hit.collider.gameObject.GetComponent(componentType);
        //             if (behaviour == null) {
        //                 behaviour = hit.collider.gameObject.GetComponentInParent(componentType);
        //             }
        //             if (behaviour != null) {
        //                 if (filter != null && filter.Count > 0 && !filter.Contains(behaviour.gameObject)) {
        //                     behaviour = null;
        //                 } else {
        //                     return true;
        //                 }
        //             }
        //         }
        //     }
        //     return false;
        // }


        ///////
        //// ScreenRaycasts
        ///////

        public class ScreenRaycasts {

            const int MAX_RAYCAST_HITS = 64;

            public Vector2 ScreenPosition { get; set; }
            public Ray Ray { get; private set; }
            public RaycastHit[] HitsAll { get; set; } = new RaycastHit[MAX_RAYCAST_HITS];
            public List<RaycastHit> HitsSorted { get; set; } = new List<RaycastHit>();
        
            public void Clear() {
                this.HitsSorted.Clear();
            }

            public void Raycast(Ray? customRay = null) {
                if (customRay.HasValue) {
                    this.Ray = customRay.Value;
                } else {
                    // if (MainCamera.Instance == null) return;
                    Camera camera = Camera.current;
                    if (camera != null) {
                        this.Ray = camera.ScreenPointToRay(this.ScreenPosition);
                    }
                }
                int hits = Physics.RaycastNonAlloc(this.Ray, this.HitsAll);
                for (int i = 0; i < hits; i++) {
                    this.HitsSorted.Add(this.HitsAll[i]);
                }
                this.HitsSorted.Sort((a,b) => a.distance.CompareTo(b.distance));
            }

            public Component GetHit(out RaycastHit? hitInfo, IEnumerable<Type> componentTypes = null, List<Component> filter = null, int layerMask = int.MaxValue) {
                if (TryGetHit(out Component hitComponent, out hitInfo, componentTypes, filter, layerMask)) {
                    return hitComponent;
                }
                return null;
            }

            public bool TryGetHit(out RaycastHit? hitInfo, int layerMask = int.MaxValue) {
                hitInfo = null;
                foreach (RaycastHit hit in this.HitsSorted) {
                    int hitLayer = 1 << hit.collider.gameObject.layer;
                    if ((layerMask & hitLayer) == hitLayer) {
                        hitInfo = hit;
                        return true;
                    }
                }
                return false;
            }

            public bool TryGetHit(out Component hitComponent, out RaycastHit? hitInfo, IEnumerable<Type> componentTypes = null, List<Component> filter = null, int layerMask = int.MaxValue) {
                hitInfo = null;
                hitComponent = null;
                foreach (RaycastHit hit in this.HitsSorted) {
                    GameObject hitObject = hit.collider.gameObject;
                    int hitLayer = 1 << hitObject.layer;
                    if ((layerMask & hitLayer) == hitLayer) {
                        
                        if (componentTypes != null) {
                            int closestDistance = int.MaxValue;
                            Component closestComponent = null;
                            foreach (Type componentType in componentTypes) {
                                Component behaviour = hitObject.GetComponentInParent(componentType);
                                if (filter != null && filter.Count > 0 && !filter.Contains(behaviour)) {
                                    // not registered for event
                                } else {
                                    if (hitObject.transform.TryCountParentDistance(behaviour.transform, out int distance)) {
                                        if (distance < closestDistance) {
                                            closestDistance = distance;
                                            closestComponent = behaviour;
                                        }
                                    }
                                }
                            }

                            if (closestComponent != null) {
                                hitInfo = hit;
                                hitComponent = closestComponent;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public T GetHit<T>(int layerMask = int.MaxValue) where T : Component {
                if (TryGetHit<T>(out T behaviour, out RaycastHit? hitInfo, null, layerMask)) {
                    return behaviour;
                }
                return null;
            }

            public bool TryGetHit<T>(out T behaviour, out RaycastHit? hitInfo, List<GameObject> filter = null, int layerMask = int.MaxValue) where T : Component {
                behaviour = null;
                if (TryGetHit(out Component _behaviour, typeof(T), out hitInfo, filter, layerMask)) {
                    behaviour = _behaviour as T;
                    return behaviour != null;
                }
                return false;
            }

            public bool TryGetHit(out Component behaviour, System.Type componentType, out RaycastHit? hitInfo, List<GameObject> filter = null, int layerMask = int.MaxValue) {
                behaviour = null;
                hitInfo = null;
                foreach (RaycastHit hit in this.HitsSorted) {
                    int hitLayer = 1 << hit.collider.gameObject.layer;
                    if ((layerMask & hitLayer) == hitLayer) {
                        hitInfo = hit;
                        behaviour = hit.collider.gameObject.GetComponent(componentType);
                        if (behaviour == null) {
                            behaviour = hit.collider.gameObject.GetComponentInParent(componentType);
                        }
                        if (behaviour != null) {
                            if (filter != null && filter.Count > 0 && !filter.Contains(behaviour.gameObject)) {
                                behaviour = null;
                            } else {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
