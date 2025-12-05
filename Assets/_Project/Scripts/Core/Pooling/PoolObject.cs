using UnityEngine;
using UnityEngine.Pool;

namespace Core.Pooling
{
    /// <summary>
    /// Component automatically added to pooled objects to manage their own state.
    /// </summary>
    [DisallowMultipleComponent]
    public class PoolObject : MonoBehaviour
    {
        private IObjectPool<GameObject> _pool;

        public void Initialize(IObjectPool<GameObject> pool)
        {
            _pool = pool;
        }

        public void ReturnToPool()
        {
            if (_pool != null && gameObject.activeInHierarchy)
            {
                _pool.Release(gameObject);
            }
            else
            {
                // Fallback if the pool is missing or object is already inactive
                Destroy(gameObject);
            }
        }
    }
}