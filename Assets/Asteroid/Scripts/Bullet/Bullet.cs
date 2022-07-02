using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    MeshRenderer bulletRenderer;

    public bool Active { get; set; } = false;


    LayerMask mask;
    private float maxTravelDistance, travelDistance, speed;
    private Vector3 startPos, prevPos, velocity;

    public void StartProjectile(Vector3 direction, float startSpeed, LayerMask layerMask)
    {
        Active = true;
        mask = layerMask;

        speed = startSpeed;
        startPos = transform.position;
        prevPos = startPos;

        velocity = direction.normalized * startSpeed;
        transform.forward = velocity.normalized;

        travelDistance = 0;
        maxTravelDistance = GameScreen.Width;
    }

    private void Update()
    {
        if (!Active)
            return;

        if (travelDistance > maxTravelDistance)
        {
            Die();
            return;
        }

        transform.localPosition = GameScreen.GetScreenClampedPosition(transform.localPosition);

        Move(Time.deltaTime);

        Vector3 direction = transform.localPosition - prevPos;

        float traveledDistance = direction.magnitude;
        travelDistance += traveledDistance;

        Ray ray = new Ray(prevPos, direction.normalized);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, traveledDistance, mask, QueryTriggerInteraction.Collide))
            HandleCollision(hit);
    }
    private void Move(float dt)
    {
        Vector3 currentPosition = transform.localPosition;

        prevPos = currentPosition;
        currentPosition += velocity * dt;
        transform.localPosition = currentPosition;
    }
    private void HandleCollision(RaycastHit hit)
    {
        Destructable target = hit.transform.GetComponent<Destructable>();

        if (target != null)
            target.Destruct();

       Die();
    }

    private void Die()
    {
        BulletPool.Instance.Return(this);
    }

    public void SetColor(Color color)
    {
        bulletRenderer.material.color = color;
    }
}
