using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all critters
public class Critters : MonoBehaviour
{
    //Critter States
    public enum CritterState
    {
        ALIVE,
        HURT,
        DEAD
    }
    [SerializeField] protected CritterState critterState;

    //Internal Critter Variables
    [SerializeField] protected int health;          //Current health
    protected float hurtDuration = 0.3f;            //Duration of hurt
    protected float hurtTimer;                      //Timer for immunity when hurt

    //Physics Variables
    protected Rigidbody2D rb;
    [SerializeField] protected Vector2 force;

    //Shared Child Variables
    public Transform player;

    //When a critter takes damage
    public void TakeDamage(int amount, bool critical)
    {
        //Disable taking damage when critter is immune
        if (hurtTimer > 0) return;

        //Reduce health by damage amount
        health -= amount;

        //Check if health is 0 or below
        if (health <= 0)
        {
            //Critter is dead
            critterState = CritterState.DEAD;
            Destroy(gameObject);
        }
        else
        {
            //Critter is hurt and immune for duration
            hurtTimer = hurtDuration;

            //Go into hurt state if critical hit
            if (critical)
            {
                critterState = CritterState.HURT;
            }
        }
    }

    //Face direction of force
    public void FaceDirection()
    {
        if (force.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (force.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1, transform.localScale.y, transform.localScale.z);
        }
    }

    //Face the player
    public void FacePlayer()
    {
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1, transform.localScale.y, transform.localScale.z);
        }
    }
}