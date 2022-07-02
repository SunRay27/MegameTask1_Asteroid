
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidPool : ComponentPool<Asteroid>
{
    public static AsteroidPool Instance { get; private set; }
    private Action<Asteroid> onAsteroidDestroyed;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    protected override void OnItemFirstCreated(Asteroid item)
    {
        item.gameObject.SetActive(false);
    }
    protected override void OnItemGet(Asteroid item)
    {
        base.OnItemGet(item);
        item.gameObject.SetActive(true);
        item.StartLargeAsteroid();
    }
    protected override void OnItemReturn(Asteroid item)
    {
        base.OnItemReturn(item);
        item.gameObject.SetActive(false);

        if (item.Dead)
        {
            onAsteroidDestroyed?.Invoke(item);
            item.Revive();
        }
    }
    public Asteroid GetItemToPosition(Vector3 position)
    {
        Asteroid toReturn = GetItem();
        toReturn.transform.position = position;
        return toReturn;
    }


    public void SetOnAsteroidDestoyedAction(Action<Asteroid> action)
    {
        onAsteroidDestroyed = action;
    }
}
