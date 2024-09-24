using System.Collections;
using System.Collections.Generic;
using System;

namespace Kuuasema.Utils {
    /**
    * A generic pool that does not need to be derived for usage:
    *   T obj = GenericPool<T>.Get();
    *   ...
    *   GenericPool<T>.Recycle(obj);
    */
    public static class GenericPool<T> where T : new() {
        private static Queue<T> queue = new Queue<T>();
        public static T Get() => GetAndInit(null);
        public static T GetAndInit(Action<T> init) {
            T t;
            if (queue.Count > 0) t = queue.Dequeue();
            else t = new T();
            if (init != null) {
                init(t);
            }
            return t;
        }
        public static void Recycle(T t) => RecycleAndCleanup(t, null);
        public static void RecycleAndCleanup(T t, Action<T> cleanup) {
            IList list = t as IList;
            if (list != null) {
                list.Clear();
            }
            if (cleanup != null) {
                cleanup(t);
            }
            queue.Enqueue(t);
        }
    }

    public static class GenericPool {
        private static Type PoolType = Type.GetType("Kuuasema.Engine.GenericPool`1");
        private static Dictionary<Type,Type> PoolTypeMap = new Dictionary<Type, Type>();
        private static object[] recycleArgs = new object[1];
        public static object GetFromPool(Type type) {
            Type poolType = GetGenericPool(type);
            return poolType.GetMethod("Get").Invoke(null, null);
        }
        public static void RecycleToPool(object obj) {
            Type type = obj.GetType();
            Type poolType = GetGenericPool(type);
            recycleArgs[0] = obj;
            poolType.GetMethod("Recycle").Invoke(null, recycleArgs);
        }
        private static Type GetGenericPool(Type type) {
            Type poolType;
            if (!PoolTypeMap.TryGetValue(type, out poolType)) {
                Type[] args = { type };
                poolType = PoolType.MakeGenericType(args);
                PoolTypeMap[type] = poolType;
            }
            return poolType;
        }
    }

    /**
    * Generic pool for arrays.
    */
    public class ArrayPool<T> where T : new() {
        private int size;
        private Queue<T[]> queue = new Queue<T[]>();
        public ArrayPool(int size) {
            this.size = size;
        }
        public T[] Get() {
            if (queue.Count > 0) return queue.Dequeue();
            return new T[this.size];
        }
        public void Recycle(T[] t) {
            for (int i = 0; i < this.size; i++) {
                t[i] = default(T);
            }
            queue.Enqueue(t);
        }
    }
}
