using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //Variables
    private Rigidbody2D rb;
    [HideInInspector] public Vector2 forceDir;
    [HideInInspector] public float speed;
    public bool playerShot = false;
    public GameObject explosionPrefab;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = forceDir * speed;
        Invoke("ExplodeImpact", 5);
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (playerShot == false)
            {
                print("hit player");
                ExplodeImpact();
            }
        }
        else if (collision.tag == "Enemy")
        {
            if (playerShot == true)
            {
                print("hit enemy");
                ExplodeImpact();
            }
        }
        //Collided with another projectile
        else if (collision.tag == "Projectile")
        {
            //If this projectile is player's
            if (playerShot == true)
            {
                //Break through and destroy enemy's projectile
                if (collision.GetComponent<Projectile>().playerShot == false)
                {
                    collision.GetComponent<Projectile>().ExplodeImpact();
                }
            }
            //This projectile is not player's
            else
            {
                //Bounce off another enemy's projectile
                if (collision.GetComponent<Projectile>().playerShot == false)
                {
                    rb.velocity *= -1;
                }
            }
            
        }
        else if (collision.tag != "Deflector")
        {
            print("hit wall vanish");
            ExplodeImpact();
        }
        
    }

    //Explode and destroy on impact
    public void ExplodeImpact()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
