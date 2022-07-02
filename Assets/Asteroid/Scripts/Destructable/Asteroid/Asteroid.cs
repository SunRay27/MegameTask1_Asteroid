using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Destructable
{
    public enum AsteroidState { Large, Medium, Small }

    public AsteroidState State { get; private set; } = AsteroidState.Large;

    [Header("Large asteroid")]
    [Range(0,360)]
    [SerializeField] float divisionAngle = 45f;
    [SerializeField] int divisionCount = 2;

    [Header("Speed and collision")]
    [SerializeField] float minStartSpeed = 5f;
    [SerializeField] float maxStartSpeed = 10f;
    [SerializeField] LayerMask collisionMask;

    [Header("Visual")]
    [SerializeField] float maxRotationSpeed = 10f;
    [SerializeField] float minRotationSpeed = 10f;
    [SerializeField] AudioClip explosionClip;

    Vector3 currentSpeed = Vector3.zero;
    Vector3 rotationSpeed;




    
    

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
        transform.localPosition += currentSpeed * Time.deltaTime;
    }
    void LateUpdate()
    {
        transform.localPosition = GameScreen.GetScreenClampedPosition(transform.localPosition);
    }
    void OnTriggerEnter(Collider col)
    {
        if (collisionMask == (collisionMask | (1 << col.gameObject.layer)))
        {
            Dead = true;
            AudioManager.Instance.PlayFast2D(explosionClip);
            AsteroidPool.Instance.Return(this);
        }
    }

    //start asteroid methods
    public void StartLargeAsteroid()
    {
        //randomize rotation and speed
        Vector2 randomDirection = Random.insideUnitCircle;
        currentSpeed = new Vector3(randomDirection.x, 0, randomDirection.y) * Random.Range(minStartSpeed, maxStartSpeed);
        rotationSpeed = new Vector3(Random.Range(minRotationSpeed, maxRotationSpeed), Random.Range(minRotationSpeed, maxRotationSpeed), Random.Range(minRotationSpeed, maxRotationSpeed));
        State = AsteroidState.Large;

        ApplyStateSize();
    }
    public void StartAsteroidManual(Vector3 direction, float speed)
    {
        //set direction and speed manually, randomize rotation
        currentSpeed = direction * speed;
        rotationSpeed = new Vector3(Random.Range(minRotationSpeed, maxRotationSpeed), Random.Range(minRotationSpeed, maxRotationSpeed), Random.Range(minRotationSpeed, maxRotationSpeed));

        ApplyStateSize();
    }

    //asteroid division logic
    void DivideAsteroid(AsteroidState newAsteroidsState)
    {
        float newAsteroidSpeed = Random.Range(minStartSpeed, maxStartSpeed);

        float horStep = divisionAngle / divisionCount;
        float curHorArc = -horStep * ((divisionCount - 1) / 2f);

        for (int i = 0; i < divisionCount; i++)
        {
            //calculate rotation
            Quaternion newRotation = Quaternion.Euler((new Vector3(0, curHorArc, 0))) * Quaternion.LookRotation(currentSpeed, Vector3.up); ;
            Vector3 rotationVector = newRotation * Vector3.forward;
            curHorArc += horStep;

            //spawn asteroid
            Asteroid newAsteroid = AsteroidPool.Instance.GetItemToPosition(transform.localPosition);
            newAsteroid.State = newAsteroidsState;
            newAsteroid.StartAsteroidManual(rotationVector, newAsteroidSpeed);
        }
    }
    void ApplyStateSize()
    {
        switch (State)
        {
            case AsteroidState.Large:
                transform.localScale = Vector3.one * 4;
                break;
            case AsteroidState.Medium:
                transform.localScale = Vector3.one * 2.5f;
                break;
            case AsteroidState.Small:
                transform.localScale = Vector3.one * 1;
                break;
        }
    }

    //asteroid hit
    public override void Destruct()
    {
        base.Destruct();

        if (State == AsteroidState.Large)
            DivideAsteroid(AsteroidState.Medium);
        else if (State == AsteroidState.Medium)
            DivideAsteroid(AsteroidState.Small);

        AudioManager.Instance.PlayFast2D(explosionClip);
        AsteroidPool.Instance.Return(this);
    }
}
