using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpioProjectile : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        //Rotate projectile towards direction
        float projectileAngle = Mathf.Atan2(player.position.y - transform.position.y, player.position.x - transform.position.x) * Mathf.Rad2Deg;
        float lerpedAngle = Mathf.Lerp(transform.rotation.z, projectileAngle, Time.deltaTime * 5f);
        transform.Rotate(0, 0, lerpedAngle);

        //Get direction of projectile to throw at
        Vector2 dir = player.position - transform.position;

        //Set normalised direction and velocity of projectile
        GetComponent<Rigidbody2D>().velocity += dir.normalized * 10f * Time.deltaTime;

    }
}
