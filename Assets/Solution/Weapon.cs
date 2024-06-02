using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    class Bullet {
        public Vector3 origin;
        public Vector3 velocity;
        public float time;
        public TrailRenderer tracer;
        public Vector3 Position(float t, Vector3 gravity) {
            return origin + (velocity*t) + (0.5f * gravity * t * t);
        }

        public bool Expired() {
            if (time > 3.0) {
                Destroy(tracer.gameObject);
                return true;
            }
            return false;
        }
    }

    public Transform raycastNode;
    public ParticleSystem muzzleFlashParticles;
    public ParticleSystem impactParticles;
    public TrailRenderer tracerTrail;

    public float bulletDrop;
    public float bulletSpeed;
    public float rateOfFire = 60; // rounds per second
    public float maxLifeTime = 3.0f;
    public int numParticles = 1;
    public bool debug;
    public bool bounce;

    List<Bullet> bullets = new List<Bullet>();
    bool isFiring = false;
    float accumulatedTime = 0.0f;

    RaycastHit hitInfo;
    Vector3 previousPosition;
    Vector3 previousDirection;

    public void StartFiring() {
        isFiring = true;
        accumulatedTime = 0.0f;
        FireBullets(Time.deltaTime);
    }

    public void StopFiring() {
        isFiring = false;
    }

    private void LateUpdate() {

        bullets.RemoveAll(b => b.Expired());

        // Update all bullet positions
        foreach (var bullet in bullets) {
            SimulateBullet(bullet, Time.fixedDeltaTime);
        }
        
        if (isFiring) {
            FireBullets(Time.fixedDeltaTime);
        }

        previousPosition = raycastNode.position;
        previousDirection = raycastNode.forward;
    }

    void FireBullets(float deltaTime) {
        accumulatedTime += deltaTime;
        float fireInterval = 1.0f / rateOfFire;
        while (accumulatedTime >= 0.0f) {
            float interpolate = accumulatedTime / deltaTime;
            Fire(accumulatedTime, interpolate);
            accumulatedTime -= fireInterval;
        }
    }

    void Fire(float deltaTime, float interpolate) {
        
        Vector3 origin = Vector3.Lerp(previousPosition, raycastNode.position, interpolate);
        Vector3 velocity = Vector3.Lerp(previousDirection, raycastNode.forward, interpolate) * bulletSpeed;
        TrailRenderer tracer = Instantiate(tracerTrail, origin, Quaternion.identity);
        tracer.AddPosition(origin);

        Bullet bullet = CreateBullet(origin, velocity, tracer, 0);
        SimulateBullet(bullet, deltaTime);

        muzzleFlashParticles.Emit(1);
    }

    Bullet CreateBullet(Vector3 position, Vector3 velocity, TrailRenderer tracer, float lifetime) {
        Bullet bullet = new Bullet {
            origin = position,
            velocity = velocity,
            tracer = tracer,
            time = lifetime
        };
        bullets.Add(bullet);
        return bullet;
    }

    void SimulateBullet(Bullet bullet, float deltaTime) {

        Vector3 p0 = bullet.Position(bullet.time, Vector3.down * bulletDrop);
        Vector3 p1 = bullet.Position(bullet.time + deltaTime, Vector3.down * bulletDrop);
        bullet.time += deltaTime;

        Vector3 rayOrigin = p0;
        Vector3 rayDirection = p1 - p0;
        float distance = rayDirection.magnitude;
        bool hit = Physics.Raycast(rayOrigin, rayDirection, out hitInfo, distance);
        if (hit) {
            impactParticles.transform.position = hitInfo.point + hitInfo.normal * 0.01f;
            impactParticles.transform.forward = hitInfo.normal;
            impactParticles.Emit(numParticles);
            bullet.time = maxLifeTime;
            bullet.tracer.transform.position = hitInfo.point;

            if (bounce) {
                bullet.origin = hitInfo.point;
                bullet.velocity = Vector3.Reflect(bullet.velocity, hitInfo.normal);
                bullet.time = 0;
            }
        } else {
            bullet.tracer.transform.position = p1;
        }

        if (debug) {
            Debug.DrawLine(p0, p1, Color.red, 1);
        }
    }
}
