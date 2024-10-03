KUU UTILS
============

ABOUT
------------
This package has no dependencies and contains useful utilities as well as simple generic features.

SETUP
------------
Add this package to your project with the Package Manager by giving it this git url. Unity will try to download the package using git. 
Depending on your machine you may have to run additional commands to make sure Unity has the permission to do it:
 > **OSX**
 > ```  
 > ssh-add
 > ```  

FEATURES
============

CUSTOM INSPECTOR ATTRIBUTES
------------

**DisableInspector (Field Attribute)**  
This will make the serialized field show up in the inspector in a disabled state.
Useful for showing values in runtime that are not supposed to be editable.  
Example:  
```
[DisableInspector]  
[SerializedField]  
private int counter;
```

**MinMaxSlider (Field Attribute)**  
This attribute works with a float, allowing you to edit it within a min max range.  
Example:  
```
[MinMaxSlider(0, 360)]  
[SerializeField]  
private float degrees;
```

UNITY OBJECT EXTENSIONS
------------

**GameObjectExtensions (static Class)**  

>**public bool TrySetActive(bool active)**  
>*Checks the active state of the game object before calling SetActive, ensuring that call is only made if the activity will change.*  

>**public GameObject CreateChild(string name)**  
>*Creates a child with given name at zeroed local position and rotation.*  

>**bool HasParent(GameObject other, int allowDepth = -1)**  
>*Checks weather the other game object is a parent of this game object, up to a maximum depth. Any negative depth is treated as max depth.*  

**TransformExtensions (static Class)**  

>**public bool TryCountParentDistance(Transform parent, out int distance)**  
>*Counts the hierarchial distance to given parent. Returns false if parameter is not a parent to this transform. Else returns true and sets the out parameter to the distance found.*  

**VectorExtensions (static Class)**  

>**bool IsDistanceCloser(Vector3 other, float distance)**  
>*Returns true if the other vector is closer than the provided distance.*  

>**bool IsPlanarDistanceCloser(Vector3 other, float distance)**  
>*Similar to the IsDistanceCloser function, except the **y** component is ignored.*  

**SerializedPropertyExtensions (static Class)**  

>**public SerializedProperty FindParentProperty()**  
>*Returns the parent property. The call is expansive so the result should be cached and at least for this method not to be called on every editor update.*  

>**public object GetTargetObject()**  
>*Returns the underlaying object that this serialized property relates to. This call is expensive and uses reflection. Not to be called frequently.*  

RUNTIME UTILITIES
------------

**RuntimeUtils (static Class)**  
This static class utilizes Unity's runtime initialization callbacks and performs the following:  
1. Before the first scene is loaded it discovers all the *[InitializeStatic]* attributes in the running application and calls them in order.  
2. After the first scene is loaded it attempts to find the *MainEntry* component from the scene.  

The class exposes three static properties:  
>public static bool IsRunning { get; private set; }  
>*Wether the application is currently running.*  

>public static bool IsQuitting { get; private set; }  
>*Wether the application is currently quitting.*  

>public static bool IsMainEntry { get; private set; }  
>*Wether the application was started from the main entry scene.*  


**InitializeStatic (Method Attribute)**  
Marking a static method with this attribute will cause it to be invoked at program startup before any scene is loaded. It supports ordering by an integer parameter, which defaults to max integer (invoked last). Attributes with same ordering will be called in an undefined order.  
Example:  
```
[InitializeStatic]  
private static void Init() { ... }

[InitializeStatic(0)]  
private static void InitFirst() { ... }

[InitializeStatic(1)]  
private static void InitSecond() { ... }  
```

**MainEntry (MonoBehaviour Class)**  
Add this component to the first scene that loads in your application. That way you can distinguish between running the full application and a standalone scene.  

**ScheduledUpdater (MonoBehaviour Class)**  
This self instantiating (if used) singleton provides access to Unity's different update functions. The different update functions can be subscribed to either as a single call or repeated. Useful for classes that dont want, need or cant hook into those updates directly.  

>**static void RunCoroutine(IEnumerator coroutine)**  
>*Starts the coroutine. Useful for scripts that dont inherit from UnityEngine.Object and cant therefore start themselves.*  

>**static void RequestUpdate(Action action)**  
>*Provided action will be called once during next Update.*  

>**static void RequestLateUpdate(Action action)**  
>*Provided action will be called once during next LateUpdate.*  

>**static void RequestFixedUpdate(Action action)**  
>*Provided action will be called once during next FixedUpdate.*  

>**static void RequestContinuousUpdate(Action action)**  
>*Provided action will be called one every subsequent Update, until cancelled.*  

>**static void RequestContinuousLateUpdate(Action action)**  
>*Provided action will be called one every subsequent LateUpdate, until cancelled.*  

>**static void RequestContinuousFixedUpdate(Action action)**  
>*Provided action will be called one every subsequent FixedUpdate, until cancelled.*  

>**static void CancelContinuousUpdate(Action action)**  
>*Cancels continous Update calls to provided action.*  

>**static void CancelContinuousLateUpdate(Action action)**  
>*Cancels continous LateUpdate calls to provided action.*  

>**static void CancelContinuousFixedUpdate(Action action)**  
>*Cancels continous FixedUpdate calls to provided action.*  

>**bool InUpdate { get; }**  
>**bool InLateUpdate { get; }**  
>**bool InFixedUpdate { get; }**  
>*These three static properties can tell in what update phase the program is currently in (if any).*  
  
**Wait (static Class)**  
This class just contains a collection of coroutine yields, for simple reusability.  

**WaitForSeconds ONE_MILLISECOND;**  
**WaitForSeconds ONE_SECOND;**  
**WaitForSeconds TEN_SECONDS;**  
**WaitForFixedUpdate FOR_FIXED_UPDATE;**  

Example:  
```
yield return Wait.ONE_SECOND;
```

POOLING AND RECYCLING
------------

**GenericPool< T > (static Class)**  
A static class and pool for any type. Since its a template class, only such pools exist that are actually invoked in the program.  
>**static T Get()**  
>*Returns an object of type T, from a pool or a new instance if not available.*  

>**static T GetAndInit(Action<T> init)**  
>*Returns an object of type T, additionally the init action is called with the object as argument.*  

>**static void Recycle(T t)**  
>*Recycles object of type T to it's corresponding static pool.*  

>**static void RecycleAndCleanup(T t, Action<T> cleanup)**  
>*Recycles object and invokes the additional cleanup action on it.*  

Example usage of the GenericPool<T>:  
```
List<Vector3> tempVectors = GenericPool<List<Vector3>>.Get();  
// do something with temp list  
GenericPool<List<Vector3>>.Recycle(tempVectors);  
```



**ArrayPool< T > (Class)**  
A (non static) class that works with fixed size arrays, the size which is determined on the constructor call.
>**ArrayPool(int size)**  
>*Creates an array pool of given size.*  

>**T[] Get()**  
>*Returns an array of type T, with the length determined by the pool instance.*  

>**void Recycle(T[] t)**  
>*Recycles the array, setting its contents to default. The default is determined by type of T; it can be null, 0, false, etc...*  

Example usage of the ArrayPool<T>:  
*Note that this example is just minimal and you should not create a new ArrayPool everytime you need one. Instead have the class that needs these temporaty arrays create the pool (and make sure its destroyed eventually).*  
```
ArrayPool<Vector3> vectorsPool = new ArrayPool<Vector3>(8);
Vector3[] tempVectors = vectorsPool.Get();  
// do something with temp array  
vectorsPool.Recycle(tempVectors);  
```

**GenericPool (static Class)**  
A static and non generic class containing methods that operate on the templated generic pool counterparts using reflection. There is therefore a slight overhead to this when compared to the templated generic pools.  
>**static object GetFromPool(Type type)**  
>*Returns an object of given type from the associated pool.*  

>**static void RecycleToPool(object obj)**  
>*Recycles the object back to the associated pool.*



**RecycleToQueue (MonoBehaviour Class)**  
A very lightweight and simple way to recycle game objects. This component exposes a Queue property that has to be set in order for it to recycle.  
>**void Recycle()**  
>*Recycles the object to the Queue if set, else does nothing.*  

>**protected virtual void OnRecycle()**  
>*Virtual method to be overridden if custom cleanup actions are needed when recycling.*  



**RecycleBin (MonoBehaviour Class)**  
This component serves as a recycle bin for game objects. It works both in runtime and editor, and is mainly useful for the editor when dealing with scenes that create a lot of temporary objects, reducing the garbage created and eventual slowdown associated with it.
This way game objects that lose scope either by a recent recompile (or any other means) will be cleaned up. If you create objects that are hidden in the hierarchy and not saved with the scene, then this will keep your scene clean.  
It works in conjuction with *RecycleToQueue* components.   
This will probably need a good example... (TODO). 
>**static void PlaceInBin(GameObject gameObject)**  
>*Places the game object in the recycle bin.*  

>**private void EmptyBin()**  
>*This is a private method, only accessible trough the context menu ("Empty Bin") of the recycle bin game object. It destroys all the game objects placed in the recycle bin.*   

>**private void EmptyGarbage()**  
>*This is a private method, only accessible trough the context menu ("Empty Garbage") of the recycle bin game object. It destroys all the game objects placed in the recycle bin, except those that have the RecycleToQueue component with a valid Queue.*  

>**private void LogContents()**  
>*This is a private method, only accessible trough the context menu ("Log Contents") of the recycle bin game object. It prints it's contents and updates the name with up to date information.*  





SINGLE BEHAVIOURS
------------


**SingleBehaviour< T > (MonoBehaviour Class)**  
This is a generec class that encapsulates T which is any class inheriting ultimately from UnityEngine.Component. The idea is that this provides automatic toggling on/off of the comonent so that only one of each type is ever active on the scene at a time. Typical problem that this solves is having an AudioListener or an EventSystem present on each scene, but can be useful for Cameras as well. This will not toggle off any such components that dont have the SingleBehaviour as well.  

>**protected virtual void OnActivate()**  
>*This is called when the component is activated. Can be overridden but the base implementation should then be called as well.*  

>**protected virtual void OnDeactivate()**  
>*This is called when the component is deactivated. Can be overridden but the base implementation should then be called as well.*  


**SingleEventSystem (MonoBehaviour Class)**  
This is a SingleBehaviour< EventSystem > class.  


**MainCamera (MonoBehaviour Class)**  
This is a SingleBehaviour< Camera > class.  

>**public static Camera Camera { get }**  
>*This provides access to the Camera object that the currently active MainCamera encapsulates.*  
```
Camera mainCam = MainCamera.Camera;  
```
*The key difference between this and Unity's own Camera.main is that this also disables and enables cameras in a predictable way, while Camera.main just returns one of the cameras tagged as Main (seemingly undeterministic) and performs no enabling and disabling of its own.*  

STATE MACHINE
------------

**StateMachine< T > (Class)**
This is a generic class where T is any Enum type, representing the states. This has a stack and supports pushing and popping states.  

**StateMachine< T >.State (Class)**
State logic is implemented by extending the State class found within the StateMachine.  

*The StateMachine will need a separate example.*  

MISCELANEOUS UTILITIES
------------

**GUID (Class)**  
A class wrapper around System.Guid allowing it to be serialized and shown in the inspector.  

**FaceCameraWorldCanvas (MonoBehaviour Class)**  
Makes a world canvas always face its camera.  

**InputRaycaster (MonoBehaviour Class)**  
A class with more advanced raycast results. 

**LayerHelper (static Class)**  
Layer name utilities.  

**MathUtils (static Class)**  

>**float SqrDistance(Vector3 a, Vector3 b)**  
>*Returns the square distance between two vectors.*  

>**bool LineIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersect, float maxDistance = float.MaxValue)**  
>*Returns the intersection point of two lines (p1,p2) and (p3,p4).*  

>**Vector3 ClosestPointOnLine(Vector3 point, Vector3 line0, Vector3 line1)**  
>*Returns the closest point on the line from provided point.*  

>**bool PointInPolygon(Vector3 point, List<Vector3> polygon)**  
>*Returns true if the point is inside the polygon.*  



**RandomSource (Class)**  

**TextUtils (static Class)**  

**Triangulator (Class)**  



SCRIPTS
============
--DisableInspectorAttribute--

--FaceCameraWorldCanvas--  

--GameObjectExtensions--

--GenericPool--

--GUID--

--InitializeStaticAttribute--

--InputRaycaster--  

--LayerHelper--  

--MainCamera--

--MainEntry--

--MathUtils--  

--MinMaxSliderAttribute--

--RandomSource--  

--RecycleBin--

--RecycleToQueue--

--RuntimeUtils--

--ScheduledUpdater--

--SerializedPropertyExtensions--

--SingleBehaviour--

--SingleEventSystem--

--StateMachine--

--TextUtils--

--TransformExtensions--

--Triangulator--

--VectorExtensions--

--Wait--  

