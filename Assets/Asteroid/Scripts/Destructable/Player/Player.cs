using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Destructable
{
    [Header("Movement")]

    [SerializeField] float maxSpeed = 15;
    [SerializeField] float accelerationRate = 10;
    [SerializeField] float rotationSpeed = 150;

    public bool UseMouseInput { get; set; } = false;


    [Header("Weapon")]
    [SerializeField] Transform firePoint;
    [SerializeField] float maxBulletsPerSecond = 3;
    [SerializeField] float bulletSpeed = 100;
    [SerializeField] Color bulletColor = Color.green;
    [SerializeField] LayerMask bulletMask;


    [field:Header("Health")]
    [field:SerializeField] public int MaxHP { get; private set; } = 4;
    public int HP { get; private set; }
    [SerializeField] float invincibilityTime = 3;
    [SerializeField] LayerMask collisionMask;


    [Header("Visual")]
    [SerializeField] ParticleSystem thrusterParticle;
    [SerializeField] AudioClip fireClip, playerLostLifeClip;
    [SerializeField] AudioSource thrusterSource;
    [SerializeField] MeshRenderer meshRenderer;


    //local vars
    private float currentRotation = 0;
    private Vector3 currentSpeed = Vector3.zero;

    private float bulletDelay;
    private float bulletTimer = 0;

    Action onPlayerDead;
    Action onPlayerLostLife;


    //Unity events
    private void Start()
    {
        bulletDelay = 1f / maxBulletsPerSecond;
    }
    void Update()
    {

        //process weapon logic
        bulletTimer += Time.deltaTime;

        if (bulletTimer > bulletDelay)
        {
            bool fireInput = (UseMouseInput && Input.GetKeyDown(KeyCode.Mouse0)) || Input.GetKeyDown(KeyCode.Space);

            if (fireInput)
            {
                FireBullet();
                bulletTimer = 0;
            }
        }


        //process movement
        float verticalAxis = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || (UseMouseInput && Input.GetKey(KeyCode.Mouse1)) ? 1 : 0;
        float horizontalAxis = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0);

        if (verticalAxis > 0)
        {
            //player is accelerating so play effects
            if (!thrusterParticle.isPlaying)
            {
                thrusterParticle.Play();
                thrusterSource.Play();
                thrusterSource.loop = true;
            }

            //increase speed vector
            currentSpeed += transform.forward * accelerationRate * Time.deltaTime;
        }
        else
        {
            //stop effects
            if (thrusterParticle.isPlaying)
            {
                thrusterParticle.Stop();
                thrusterSource.loop = false;
            }
        }


        if (UseMouseInput)
        {
            Vector3 mousePoint = GameScreen.GetMousePositionInWorldSpace();
            //project on Vector3.up just in case player is not on 0 y coordinate
            //Vector3 targetDirection = mousePoint - Vector3.ProjectOnPlane(transform.localPosition, Vector3.up);

            Vector3 targetDirection = mousePoint - transform.localPosition;

            //dead zone is meant to get rid of infinite rotation oscillations close to target angle
            float deadZoneAngle = 0.5f;
            float angle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
            if (Mathf.Abs(angle) > deadZoneAngle)
            {
                if (angle > 0)
                    currentRotation += rotationSpeed * Time.deltaTime;
                else
                    currentRotation -= rotationSpeed * Time.deltaTime;
            }
        }
        else
        {
            currentRotation += rotationSpeed * horizontalAxis * Time.deltaTime;
        }

        //clamp resulting speed vector magnitude
        if(currentSpeed.sqrMagnitude > maxSpeed * maxSpeed)
            currentSpeed = Vector3.ClampMagnitude(currentSpeed, maxSpeed);

        //apply movement
        transform.localRotation = Quaternion.AngleAxis(currentRotation, Vector3.up);
        transform.localPosition += currentSpeed * Time.deltaTime;
    }
    private void LateUpdate()
    {
        //clamp player position on screen
        transform.localPosition = GameScreen.GetScreenClampedPosition(transform.localPosition);
    }
    private void OnTriggerEnter(Collider col)
    {
        if (collisionMask == (collisionMask | (1 << col.gameObject.layer)))
            if (!Dead && !Invincible)
                Destruct();
    }



    public override void Revive()
    {
        base.Revive();
        Invincible = false;
        meshRenderer.enabled = true;
        HP = MaxHP;

        StopAllCoroutines();
        StopMovement();

        gameObject.SetActive(true);
        

        //
        // Not sure if it is really needed (reference game doesn't have this feature, but in docs it is said that player should be invincible when it spawns) 
        //
        
        //make invincible for 3 seconds
        StartCoroutine(MakeInvincibleCoroutine(invincibilityTime));
        //play new player sound
        AudioManager.Instance.PlayFast2D(playerLostLifeClip);
    }
    public override void Destruct()
    {
        if (Invincible)
            return;

        HP--;
        if (HP <= 0) //gameover
        {
            //mark as dead
            base.Destruct();
            gameObject.SetActive(false);

            //call event on game manager
            onPlayerDead?.Invoke();
        }
        else
        {
            //call event on game manager
            onPlayerLostLife?.Invoke();
            
            //stop movement and assign new position on screen
            StopMovement();
            transform.localPosition = GameScreen.GetRandomPointOnScreen();

            //make invincible for 3 seconds
            StartCoroutine(MakeInvincibleCoroutine(invincibilityTime));

            //play new player sound
            AudioManager.Instance.PlayFast2D(playerLostLifeClip);
        }
    }
    IEnumerator MakeInvincibleCoroutine(float seconds)
    {
        Invincible = true;
        float startTime = Time.time;
        float endTime = startTime + seconds;

        // 2 full blinks per second
        float waitTime = 0.25f;

        while (Time.time < endTime)
        {
            meshRenderer.enabled = !meshRenderer.enabled;

            //because invincibilty time may be different
            if (endTime - Time.time > waitTime)
                yield return new WaitForSeconds(waitTime);
            else
                yield return new WaitForSeconds(endTime - Time.time);
        }
        meshRenderer.enabled = true;
        Invincible = false;
    }

    void FireBullet()
    {
        Bullet newBullet = BulletPool.Instance.GetItemToPosition(firePoint.position);
        newBullet.SetColor(bulletColor);
        newBullet.StartProjectile(firePoint.forward, bulletSpeed, bulletMask);

        AudioManager.Instance.PlayFast2D(fireClip);
    }
    public void StopMovement()
    {
        currentRotation = 0;
        currentSpeed = Vector3.zero;
    }
    
    //hook GameManager actions
    public void SetOnPlayerDeadAction(Action act) {onPlayerDead = act;}
    public void SetOnPlayerLostLifeAction(Action act) {onPlayerLostLife = act;}
}
