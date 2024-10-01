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



POOLING AND RECYCLING
------------

**GenericPool<T> (static Class)**  
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



**ArrayPool<T> (Class)**  
A (non static) class that works with fixed size arrays, the size which is determined on the constructor call.
>**ArrayPool(int size)**  
>*Creates an array pool of given size.*  

>**T[] Get()**  
>*Returns an array of type T, with the length determined by the pool instance.*  

>**void Recycle(T[] t)**  
>*Recycles the array, setting its contents to default. The default is determined by type of T; it can be null, 0, false, etc...*  

Example usage of the ArrayPool<T>:  
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
>*This is a private method, only accessible trough the context menu ("Log Contents") of the recycle bin game object. It destroys all the game objects placed in the recycle bin.*   

>**private void EmptyGarbage()**  
>*This is a private method, only accessible trough the context menu ("Empty Garbage") of the recycle bin game object. It destroys all the game objects placed in the recycle bin, except those that have the RecycleToQueue component with a valid Queue.*  

>**private void LogContents()**  
>*This is a private method, only accessible trough the context menu ("Log Contents") of the recycle bin game object. It prints it's contents and updates the name with up to date information.*  





SINGLE BEHAVIOURS
------------



STATE MACHINE
------------



MISCELANEOUS UTILITIES
------------



SCRIPTS
============
--DisableInspectorAttribute--

**FaceCameraWorldCanvas**

--GameObjectExtensions--

--GenericPool--

**GUID**

--InitializeStaticAttribute--

**InputRaycaster**

**LayerHelper**

**MainCamera**

--MainEntry--

**MathUtils**

--MinMaxSliderAttribute--

**RandomSource**

--RecycleBin--

--RecycleToQueue--

--RuntimeUtils--

--ScheduledUpdater--

--SerializedPropertyExtensions--

**SingleBehaviour**

**SingleEventSystem**

**StateMachine**

**TextUtils**

--TransformExtensions--

**Triangulator**

**VectorExtensions**

**Wait**


