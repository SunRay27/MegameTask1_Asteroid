using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentPool<T> : MonoBehaviour where T : Component
{
    [field: SerializeField]
    protected int PoolCapacity { get; private set; } = 100;

    [field: SerializeField]
    public Transform Prefab { get; private set; }

    public List<T> AvailableItems { get; private set; } = new List<T>();
    public List<T> InUse { get; private set; } = new List<T>();

    protected virtual void Awake()
    {
        InUse = new List<T>();

        for (int i = 0; i < PoolCapacity; i++)
            AvailableItems.Add(CreateItem());
    }
    public virtual void Return(T obj)
    {
        if (AvailableItems.Count < PoolCapacity)
            OnItemReturn(obj);
        else if (!AvailableItems.Contains(obj))
            OnItemDestroy(obj);
    }
    public virtual T GetItem()
    {
        T obj = GetItemRaw();
        OnItemGet(obj);

        return obj;
    }

    protected T GetItemRaw()
    {
        T obj;
        if (AvailableItems.Count > 0)
        {
            obj = AvailableItems[0];
            AvailableItems.RemoveAt(0);
            InUse.Add(obj);
        }
        else
        {
            obj = CreateItem();
            InUse.Add(obj);
        }
        return obj;
    }
    protected virtual void OnItemGet(T item) { }
    protected virtual void OnItemReturn(T item)
    {
        AvailableItems.Add(item);
        InUse.Remove(item);
    }
    protected virtual void OnItemDestroy(T item)
    {
        InUse.Remove(item);
        Destroy(item.gameObject);
    }


    public virtual void ReturnAll()
    {
        while (InUse.Count > 0)
            Return(InUse[0]);
    }

    protected virtual void OnItemFirstCreated(T item) { }

    private T CreateItem()
    {
        T spawned = Instantiate(Prefab).GetComponent<T>();
        spawned.transform.SetParent(transform, false);
        OnItemFirstCreated(spawned);

        return spawned;
    }
}
