using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : Destructable
{
    [Header("Weapon")]
    [SerializeField] float minbulletDelay = 2f;
    [SerializeField] float maxbulletDelay = 5f;
    [SerializeField] float bulletSpeed = 100;
    [SerializeField] Color bulletColor = Color.red;
    [SerializeField] LayerMask bulletMask;


    [Header("Collision")]
    [SerializeField] LayerMask collisionMask;

    [Header("Audio")]
    [SerializeField] AudioClip fireClip;
    [SerializeField] AudioClip explosionClip;

    public Transform ShootTarget { get; set; }



    private Action onUFODestroyed;
    private float bulletDelay;
    private float bulletTimer = 0;

    private float moveSpeed;
    private Vector3 direction;
    private float maxTravelDistance;
    private float travelDistance;
    
    void Update()
    {
        if (Dead)
            return;

        if(travelDistance > maxTravelDistance)
        {
            Destruct();
            return;
        }

        bulletTimer += Time.deltaTime;

        if(bulletTimer > bulletDelay)
        {
            //fire bullet
            Bullet newBullet = BulletPool.Instance.GetItemToPosition(transform.localPosition);
            newBullet.SetColor(bulletColor);
            newBullet.StartProjectile(ShootTarget.localPosition - transform.localPosition, bulletSpeed, bulletMask);

            //play fire sound
            AudioManager.Instance.PlayFast2D(fireClip);

            //reset timer and randomize delay
            bulletTimer = 0;
            bulletDelay = UnityEngine.Random.Range(2f, 5f);
        }
        


        //translate ufo
        transform.localPosition += direction * moveSpeed * Time.deltaTime;
        travelDistance += moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (collisionMask == (collisionMask | (1 << col.gameObject.layer)))
            Destruct();
    }

    public override void Revive()
    {
        base.Revive();

        Vector3 moveDirection;
        Vector3 pos = GameScreen.GetRandomPointForUFO(out moveDirection);

        transform.localPosition = pos;
        travelDistance = 0;
        maxTravelDistance = GameScreen.Width;

        //10 seconds per screen width
        moveSpeed = maxTravelDistance / 10;
        direction = moveDirection;
        gameObject.SetActive(true);

        //reset bullet timer and delay
        bulletTimer = 0;
        bulletDelay = UnityEngine.Random.Range(minbulletDelay, maxbulletDelay);
    }
    public override void Destruct()
    {
        base.Destruct();
        gameObject.SetActive(false);
        onUFODestroyed?.Invoke();
        AudioManager.Instance.PlayFast2D(explosionClip);
    }


    public void SetOnUFODestoyedAction(Action action)
    {
        onUFODestroyed = action;
    }
}
