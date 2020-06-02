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
    [Header("Component")]
    public string componentClassType = "";

    private Type type;
    private List<GameObject> objects = new List<GameObject>();
    private List<Component> components = new List<Component>();

    private void Start()
    {
        Init();
    }

    public void Init()
    {
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
}
