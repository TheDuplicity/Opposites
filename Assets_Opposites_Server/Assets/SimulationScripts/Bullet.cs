using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float range;
    public Vector3 spawnPos;
    private float damage;
    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.position += (velocity * Time.deltaTime);

        Vector3 displacement = (transform.position - spawnPos);
        float distance = displacement.magnitude;
        if (distance > range)
        {
            Destroy(gameObject);
        }
    }

    public float dealDamage()
    {
        return damage;
    }

    public void createBulletData(float setRange, float setSpeed ,float setDamage, Vector3 setSpawnPos, Quaternion setRotation)
    {
        range = setRange;
        damage = setDamage;
        spawnPos = setSpawnPos;
        transform.position = setSpawnPos;

        transform.rotation = setRotation;
        velocity = new Vector3(0, 1, 0);
        velocity.Normalize();
        velocity = setRotation * velocity;

        velocity *= setSpeed;
    }

}
