
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : ComponentPool<AudioSource>
{
    protected override void OnItemGet(AudioSource item)
    {
        base.OnItemGet(item);
        item.gameObject.SetActive(true);
    }
    protected override void OnItemReturn(AudioSource item)
    {
        base.OnItemReturn(item);
        item.gameObject.SetActive(false);
        item.playOnAwake = false;
    }
    public AudioSource GetItemToPosition(Vector3 position)
    {
        AudioSource toReturn = GetItem();
        toReturn.transform.position = position;
        return toReturn;
    }
}
