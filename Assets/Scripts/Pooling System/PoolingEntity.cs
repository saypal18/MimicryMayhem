using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Universal entity pooling component.
/// 
/// Usage:
/// 1. Call PoolingEntity.Spawn(prefab) instead of Instantiate(prefab).
///    Example: GameObject enemy = PoolingEntity.Spawn(enemyPrefab, position, rotation);
/// 
/// 2. Call entity.Despawn() instead of Destroy(gameObject).
///    Example: GetComponent<PoolingEntity>().Despawn();
/// 
/// Notes:
/// - You do NOT need to manually add this component to your prefabs. It is automatically added at runtime if missing.
/// - The system uses the Prefab's Instance ID as the unique pool identifier.
/// - Inactive entities are stored in a Stack for O(1) retrieval performance.
/// </summary>
public class PoolingEntity : MonoBehaviour
{
    // Dictionary mapping Prefab Instance IDs to Stacks of inactive game objects.
    // Using a Stack ensures O(1) performance for retrieving the most recently despawned entity.
    private static readonly Dictionary<int, Stack<GameObject>> entityPools = new();

    // The Unique ID of the pool this entity belongs to (derived from the source Prefab's Instance ID).
    private int poolId;

    // Track state explicitly to separate "active in hierarchy" from "active in game logic"
    private bool isPooled = false;

    /// <summary>
    /// Event raised when this entity is spawned (retrieved from pool or instantiated).
    /// Subscribe to this event to perform initialization logic (e.g., resetting health, playing entry animations).
    /// </summary>
    public event Action OnSpawned;

    /// <summary>
    /// Event raised when this entity is about to be despawned (returned to pool).
    /// Subscribe to this event to perform cleanup logic (e.g., stopping particles, resetting states).
    /// </summary>
    public event Action OnDespawning;

    /// <summary>
    /// Spawns an instance of the prefab from the pool at (0,0,0) with identity rotation.
    /// If the pool is empty, a new instance is created.
    /// </summary>
    /// <param name="prefab">The source prefab to spawn.</param>
    /// <returns>The spawned GameObject.</returns>
    public static GameObject Spawn(GameObject prefab)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
    }

    /// <summary>
    /// Spawns an instance of the prefab from the pool at the specified position and rotation.
    /// If the pool is empty, a new instance is created.
    /// </summary>
    /// <param name="prefab">The source prefab to spawn.</param>
    /// <param name="position">World position for the spawned instance.</param>
    /// <param name="rotation">World rotation for the spawned instance.</param>
    /// <returns>The spawned GameObject.</returns>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Spawn(prefab, position, rotation, null);
    }

    /// <summary>
    /// Spawns an instance of the prefab from the pool and sets it as a child of the specified parent.
    /// Retains the prefab's local position, rotation, and scale relative to the parent.
    /// If the pool is empty, a new instance is created.
    /// </summary>
    /// <param name="prefab">The source prefab to spawn.</param>
    /// <param name="parent">The transform to set as parent.</param>
    /// <returns>The spawned GameObject.</returns>
    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolingEntity: Cannot spawn null prefab!");
            return null;
        }

        GameObject entityObj = GetFromPool(prefab);

        if (entityObj != null)
        {
            // Reuse existing object: Reset transform to match prefab's local settings relative to new parent
            entityObj.transform.SetParent(parent, false);
            entityObj.transform.SetLocalPositionAndRotation(prefab.transform.localPosition, prefab.transform.localRotation);
            entityObj.transform.localScale = prefab.transform.localScale;
            entityObj.SetActive(true);
        }
        else
        {
            // Create new instance directly parented
            entityObj = Instantiate(prefab, parent);
            InitializeNewEntity(entityObj, prefab.GetInstanceID());
        }

        InvokeSpawnEvent(entityObj);
        return entityObj;
    }

    /// <summary>
    /// Spawns an instance of the prefab from the pool at the specified world position/rotation and parents it.
    /// If the pool is empty, a new instance is created.
    /// </summary>
    /// <param name="prefab">The source prefab to spawn.</param>
    /// <param name="position">World position.</param>
    /// <param name="rotation">World rotation.</param>
    /// <param name="parent">The transform to set as parent.</param>
    /// <returns>The spawned GameObject.</returns>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolingEntity: Cannot spawn null prefab!");
            return null;
        }

        GameObject entityObj = GetFromPool(prefab);

        if (entityObj != null)
        {
            // Reuse existing object: Parent logic differs slightly when world position is explicit
            entityObj.transform.SetParent(parent);
            entityObj.transform.SetPositionAndRotation(position, rotation);
            entityObj.SetActive(true);
        }
        else
        {
            // Create new instance
            entityObj = Instantiate(prefab, position, rotation, parent);
            InitializeNewEntity(entityObj, prefab.GetInstanceID());
        }

        InvokeSpawnEvent(entityObj);
        return entityObj;
    }

    // -------------------------------------------------------------------------
    // GENERIC OVERLOADS (For Component types)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns an entity of component type T from the pool.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="prefab">The source component prefab.</param>
    /// <returns>The component T on the spawned instance.</returns>
    public static T Spawn<T>(T prefab) where T : Component
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
    }

    /// <summary>
    /// Spawns an entity of component type T from the pool at the specified position and rotation.
    /// </summary>
    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return Spawn(prefab, position, rotation, null);
    }

    /// <summary>
    /// Spawns an entity of component type T from the pool and parents it.
    /// </summary>
    public static T Spawn<T>(T prefab, Transform parent) where T : Component
    {
        if (prefab == null) return null;
        GameObject obj = Spawn(prefab.gameObject, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    /// <summary>
    /// Spawns an entity of component type T from the pool at specified world coordinates and parents it.
    /// </summary>
    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
    {
        if (prefab == null) return null;
        GameObject obj = Spawn(prefab.gameObject, position, rotation, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    // -------------------------------------------------------------------------
    // STATIC DESPAWN METHODS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Despawns the specified GameObject. 
    /// If the object has a PoolingEntity component, it is returned to the pool.
    /// Otherwise, it is destroyed using Object.Destroy().
    /// </summary>
    /// <param name="instance">The GameObject to despawn.</param>
    public static void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (instance.TryGetComponent(out PoolingEntity entity))
        {
            entity.Despawn();
        }
        else
        {
            Destroy(instance);
        }
    }

    /// <summary>
    /// Despawns the specified PoolingEntity instance, returning it to the pool.
    /// </summary>
    /// <param name="entity">The entity to despawn.</param>
    public static void Despawn(PoolingEntity entity)
    {
        if (entity != null)
        {
            entity.Despawn();
        }
    }

    /// <summary>
    /// Despawns the entity associated with the specified component.
    ///Wrapper for Despawn(GameObject).
    /// </summary>
    /// <typeparam name="T">Any Component type.</typeparam>
    /// <param name="component">The component to despawn.</param>
    public static void Despawn<T>(T component) where T : Component
    {
        if (component != null)
        {
            Despawn(component.gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // INTERNAL HELPERS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to retrieve an inactive object from the pool associated with the prefab.
    /// </summary>
    private static GameObject GetFromPool(GameObject prefab)
    {
        int poolId = prefab.GetInstanceID();
        if (entityPools.TryGetValue(poolId, out Stack<GameObject> pool))
        {
            while (pool.Count > 0)
            {
                // Pop the last element (LIFO) for O(1) access
                GameObject pooled = pool.Pop();
                if (pooled != null)
                {
                    // Mark as no longer pooled before returning
                    if (pooled.TryGetComponent(out PoolingEntity entity))
                    {
                        entity.isPooled = false;
                    }
                    return pooled;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Initializes a newly instantiated entity with its pool ID.
    /// Adds the Entity component if missing.
    /// </summary>
    private static void InitializeNewEntity(GameObject entityObj, int poolId)
    {
        if (!entityObj.TryGetComponent(out PoolingEntity entityComponent))
        {
            entityComponent = entityObj.AddComponent<PoolingEntity>();
        }
        entityComponent.poolId = poolId;
        entityComponent.isPooled = false; // Ensure explicit state is reset for new instances
    }

    /// <summary>
    /// Invokes the OnSpawned event on the entity.
    /// </summary>
    private static void InvokeSpawnEvent(GameObject entityObj)
    {
        if (entityObj.TryGetComponent(out PoolingEntity entityComp))
        {
            entityComp.OnSpawned?.Invoke();
        }
    }

    /// <summary>
    /// Despawns this entity, deactivating it and returning it to the pool.
    /// Call this when the object should be removed from the scene.
    /// </summary>
    public void Despawn()
    {
        if (isPooled) return; // Prevent double despawn checks based on pool state, not active state

        // Raise the OnDespawning event to notify subscribers
        OnDespawning?.Invoke();

        // Deactivate GameObject
        gameObject.SetActive(false);

        // Mark as pooled
        isPooled = true;

        // Return to pool (strictly inactive objects only)
        if (!entityPools.TryGetValue(poolId, out Stack<GameObject> pool))
        {
            pool = new();
            entityPools[poolId] = pool;
        }
        pool.Push(gameObject);
    }
}
