using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kuuasema.Utils {

    public static class RuntimeUtils {

        public static bool IsRunning { get; private set; }
        public static bool IsQuitting { get; private set; }
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeApplication() {
            IsRunning = true;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            List<InitializeStaticAttribute> attributeOrder = new List<InitializeStaticAttribute>();
            
            Assembly utilsAssembly = Assembly.GetAssembly(typeof(RuntimeUtils));
            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly mainAssembly = Assembly.GetAssembly(typeof(UnityEngine.Object));
            foreach (Assembly assembly in allAssemblies) {
                bool useAssembly = assembly == mainAssembly || assembly.FullName.Contains("Assembly-CSharp");
                if (!useAssembly) {
                    foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies()) {
                        if (referencedAssembly.FullName == utilsAssembly.FullName) {
                            useAssembly = true;
                            break;
                        }
                    }
                }
                if (useAssembly) {
                    foreach (Type type in assembly.GetTypes()) {
                        MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        foreach (MethodInfo method in methods) {
                            InitializeStaticAttribute attribute = Attribute.GetCustomAttribute(method, typeof(InitializeStaticAttribute)) as InitializeStaticAttribute;
                            if (attribute != null) {
                                attribute.Method = method;
                                attributeOrder.Add(attribute);
                            }
                        }
                    }
                }
            }

            Type typeCoroutine = typeof(IEnumerator);

            attributeOrder.Sort((a,b) => a.Order.CompareTo(b.Order));
            foreach (InitializeStaticAttribute attribute in attributeOrder) {
                MethodInfo method = attribute.Method;
                if (method.ReturnType == typeCoroutine) {
                    ScheduledUpdater.RunCoroutine(method.Invoke(null, null) as IEnumerator);
                } else {
                    method.Invoke(null, null);
                }
            }

            Application.quitting += () => IsQuitting = true;

            stopwatch.Stop();
            Debug.Log($"InitializeApplication took {stopwatch.Elapsed}");
        }

        public static bool IsMainEntry { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeMainEntry() {
            if (GameObject.FindObjectOfType<MainEntry>()) {
                IsMainEntry = true;
            }
        }
    }
}
