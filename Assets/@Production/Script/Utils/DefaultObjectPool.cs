using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Pool
{
    public class DefaultObjectPool<T> : IObjectPool<T> where T : MonoBehaviour
    {
        T prefab;
        Transform parent;

        ObjectPool<T> pool;

        public int CountInactive => throw new NotImplementedException();

        public DefaultObjectPool(T prefab, Transform parent = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.pool = new ObjectPool<T>(OnCreate, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionCheck, defaultCapacity, maxSize);
        }

        public T Get()=> pool.Get();

        public PooledObject<T> Get(out T v)
        {
            v = Get();
            return new PooledObject<T>();
        }

        public void Release(T item) => pool.Release(item);

        public void Clear() => pool.Clear();

        private T OnCreate() 
        {
            return GameObject.Instantiate<T>(prefab, parent);
        }

        // Called when an item is returned to the pool using Release
        private void OnReturnedToPool(T system)
        {
            system.gameObject.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        private void OnTakeFromPool(T system)
        {
            system.gameObject.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        private void OnDestroyPoolObject(T system)
        {
            GameObject.Destroy(system.gameObject);
        }
    }
}
