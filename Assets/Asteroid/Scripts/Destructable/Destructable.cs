using System;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    public bool Dead { get; protected set; } = false;

    public bool Invincible { get; protected set; } = false;

    public virtual void Revive()
    {
        Dead = false;
    }
    public virtual void Destruct()
    {
        Dead = true;
    }
}
