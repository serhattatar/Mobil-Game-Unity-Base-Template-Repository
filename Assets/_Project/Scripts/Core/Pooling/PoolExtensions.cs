using UnityEngine;

namespace Core.Pooling
{
    public static class PoolExtensions
    {
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            // Direct Static Call - No Instance Reference visible
            return PoolManager.Spawn(prefab, position, rotation, parent);
        }

        public static void ReturnToPool(this GameObject instance)
        {
            if (instance.TryGetComponent(out PoolObject poolObj))
            {
                poolObj.ReturnToPool();
            }
            else
            {
                Object.Destroy(instance);
            }
        }

        public static void ReturnToPool(this GameObject instance, float delay)
        {
            // Uses the UniTask version internally
            PoolManager.ReturnWithDelay(instance, delay);
        }
    }
}