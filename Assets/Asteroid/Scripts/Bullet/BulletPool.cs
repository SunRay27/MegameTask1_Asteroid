
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : ComponentPool<Bullet>
{
    public static BulletPool Instance { get; private set; }

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
    }
    protected override void OnItemGet(Bullet item)
    {
        base.OnItemGet(item);
        item.gameObject.SetActive(true);
    }
    protected override void OnItemReturn(Bullet item)
    {
        base.OnItemReturn(item);
        item.Active = false;
        item.gameObject.SetActive(false);
    }
    protected override void OnItemFirstCreated(Bullet item)
    {
        item.gameObject.SetActive(false);
    }
    public Bullet GetItemToPosition(Vector3 position)
    {
        Bullet toReturn = GetItem();
        toReturn.transform.position = position;
        return toReturn;
    }
}
