using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject objectToPool;
    public string poolName = "";
    public int startPoolSize = 0;
    public bool autoGrow = false;
    public int ActiveCount { private set; get; }
    [Header("Component")]
    public string componentClassType = "";

    private Type type;
    private List<GameObject> objects = new List<GameObject>();
    private List<Component> components = new List<Component>();
    private GameObject poolingManagerReference = null;

    #region Pooling manager
    private static Dictionary<string, ObjectPooler> poolers = new Dictionary<string, ObjectPooler>();
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Start()
    {

    }

    public void Init()
    {
        CreatePoolingManager();
        if (objectToPool == null)
        {
            Debug.LogWarning($"[POOLER] No object to pool has been provided.");
            return;
        }

        SetType();

        for (int i = 0; i < startPoolSize; i++)
        {
            AddObjectToPool();
        }

        poolers.Add(poolName, this);
    }

    private void Update()
    {
        ActiveCount = 0;
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].activeInHierarchy)
            {
                ActiveCount++;
            }
        }
    }

    public GameObject SpawnObject()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i].activeInHierarchy)
            {
                objects[i].SetActive(true);
                return objects[i];
            }
        }

        if (autoGrow == false)
        {
            return null;
        }

        return AddObjectToPool(true);
    }

    public T SpawnObject<T>() where T : Component
    {
        return SpawnObject().GetComponent<T>();
    }

    public Component SpawnComponent()
    {
        if (type == null)
        {
            Debug.LogError($"[POOLER] Cannot use SpawnComponent() no componentClassType has been set!");
            return null;
        }

        if (components.Count == 0)
        {
            Debug.LogWarning($"[POOLER] Does not have components of type {componentClassType}");
            return null;
        }

        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i].activeInHierarchy)
            {
                objects[i].SetActive(true);
                return components[i];
            }
        }

        if (autoGrow == false)
        {
            return null;
        }

        AddObjectToPool(true);
        return components[components.Count - 1];
    }

    public int CountPooledObjects()
    {
        return objects.Count;
    }

    private GameObject AddObjectToPool(bool setActive = false)
    {
        GameObject temp = Instantiate(objectToPool);
        temp.SetActive(setActive);
        temp.transform.SetParent(poolingManagerReference.transform);
        temp.transform.localPosition = Vector3.zero;
        objects.Add(temp);
        PopulateComponentPool(temp);
        return objects[objects.Count - 1];
    }

    private void PopulateComponentPool(GameObject o)
    {
        SetType();
        Component component = o.GetComponent(type);
        components.Add(component);
    }

    private void SetType()
    {
        if (type != null)
        {
            return;
        }

        if (componentClassType == "")
        {
            Debug.LogWarning($"[POOLER] No Component Class provided!");
        }
        else
        {
            type = Type.GetType(componentClassType);
        }
    }

    private void CreatePoolingManager()
    {
        GameObject existingPoolingManager = GameObject.FindGameObjectWithTag("PoolingManager");

        if (existingPoolingManager != null)
        {
            poolingManagerReference = existingPoolingManager;
            return;
        }

        GameObject poolingManager = new GameObject("PoolingManager");
        poolingManager.tag = "PoolingManager";
        poolingManager.transform.position = Vector3.down * 1000;

        poolingManagerReference = poolingManager;
    }

    #region Static methods for object poolers management
    public static ObjectPooler GetObjectPoolerByPoolName(string poolName)
    {
        ObjectPooler[] poolers = GameObject.FindObjectsOfType<ObjectPooler>() as ObjectPooler[];

        for (int i = 0; i < poolers.Length; i++)
        {
            if (poolers[i].poolName == poolName)
            {
                return poolers[i];
            }
        }

        return null;
    }

    public static GameObject SpawnObject(string poolerName)
    {
        if (!poolers.ContainsKey(poolerName))
        {
            Debug.LogError($"[POOLER] Cannot use SpawnComponent() no ObjectPooler named {poolerName} found!");
            return null;
        }

        return poolers[poolerName].SpawnObject();
    }

    public static T SpawnObject<T>(string poolerName) where T : Component
    {
        if (!poolers.ContainsKey(poolerName))
        {
            Debug.LogError($"[POOLER] Cannot use SpawnComponent() no ObjectPooler named {poolerName} found!");
            return null;
        }

        return SpawnObject(poolerName).GetComponent<T>();
    }

    public Component SpawnComponent(string poolerName)
    {
        if (!poolers.ContainsKey(poolerName))
        {
            Debug.LogError($"[POOLER] Cannot use SpawnComponent() no ObjectPooler named {poolerName} found!");
            return null;
        }

        return poolers[poolerName].SpawnComponent();
    }
    #endregion
}
