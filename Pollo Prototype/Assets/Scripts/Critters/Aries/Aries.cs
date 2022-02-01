using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aries : Critters
{
    //Aries States
    private enum AriesState
    {
        IDLE,
        ALERT,
        CHASE,
        ATTACKING,
        VANISH,
        TELEPORT
    }
    [SerializeField] private AriesState ariesState = AriesState.IDLE;

    //State Timers
    private float alertTimer;
    private float alertDuration = 0.75f;
    private float chargeTimer;
    private float chargeDuration = 1.083f;
    private float skillTimer;
    private float skillDuration = 5f;

    //Variables
    public float alertDist = 5f;        //Distance to trigger alert
    public float closestDist = 1.5f;    //Closest distance it wants to get to the player

    private float speed = 10000f;
    private float maxVelocity = 1.2f;
    private LayerMask groundLayer;
    private float checkRadius = 2f;

    private Animator anim;

    public GameObject projectilePrefab;
    private float projectileSpeed = 10f;

    void Start()
    {
        critterState = CritterState.ALIVE;
        alertTimer = alertDuration;
        chargeTimer = chargeDuration;
        skillTimer = skillDuration;

        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateCritterLogic();

        //Check if there is an animator
        if (anim != null)
        {
            //Update sprite based animations
            UpdateAnimation();
        }
        else
        {
            //Update procedural animations
            UpdateProceduralAnimation();
        }
    }

    private void FixedUpdate()
    {
        //Check the ground to prevent being grounded
        if (CheckGround(checkRadius))
        {
            force += Vector2.up * speed * Time.deltaTime;
        }

        //Add final force to rigidbody
        rb.AddForce(force);
        force = Vector2.zero;

        //Clamp Exceptions
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxVelocity, maxVelocity), Mathf.Clamp(rb.velocity.y, -maxVelocity, maxVelocity));

    }

    //Critter internal behaviour loop
    public void UpdateCritterLogic()
    {
        //Hurt immunity priority, critter cannot take damage before immunity ends
        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0)
        {
            hurtTimer = 0;

            //Recover from hurt state
            if (critterState == CritterState.HURT)
            {
                critterState = CritterState.ALIVE;
                ariesState = AriesState.ALERT;
            }

            //Goes into alert
            //scorpioState = ScorpioState.ALERT;
        }

        //Update Scorpio logic
        if (critterState == CritterState.ALIVE)
        {
            UpdateAriesLogic();
        }
    }

    //Scorpio internal behaviour loop
    public void UpdateAriesLogic()
    {
        switch (ariesState)
        {
            case AriesState.IDLE:
                //Check if player is within range switch to alert
                if (TargetWithinRange(player) != 0)
                {
                    ariesState = AriesState.ALERT;
                }
                break;
            case AriesState.ALERT:
                //Countdown alert timer and switch to chase
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0)
                {
                    switch (TargetWithinRange(player))
                    {
                        case 0: //Player has gotten too far away
                            //Return to idle spot
                            ariesState = AriesState.IDLE;
                            break;
                        case 1: //Player is within chase distance
                        case 2:
                        case 3:
                            ariesState = AriesState.CHASE;
                            break;
                        default: //Should not enter default
                            ariesState = AriesState.IDLE;
                            break;
                    }
                    alertTimer = alertDuration;
                }
                FacePlayer();
                break;
            case AriesState.CHASE:
                //Countdown skill timer and use a skill when possible
                skillTimer -= Time.deltaTime;
                if (skillTimer <= 0 && TargetWithinRange(player) == 2)
                {
                    int attackSelection = Random.Range(0, 2);
                    attackSelection = 1;
                    if (attackSelection == 0)
                    {
                        ariesState = AriesState.ATTACKING;
                    }
                    else if (attackSelection == 1)
                    {
                        ariesState = AriesState.VANISH;
                        rb.velocity = Vector2.zero;
                    }

                    //Reset skill timer
                    skillTimer = skillDuration;
                }

                //Range check
                switch (TargetWithinRange(player))
                {
                    case 0: //Player has gotten too far away
                            //Hovers in place before returning to idle spot
                        ariesState = AriesState.ALERT;
                        break;
                    case 1: //Player is within chase distance
                        ChaseTarget(player, true);
                        break;
                    case 2: 
                        break;
                    case 3: //Player is too close
                        ChaseTarget(player, false);
                        break;
                    default:
                        break;
                }

                //Face the player while chasing and hovering around them
                FacePlayer();
                break;

            case AriesState.ATTACKING:
                chargeTimer -= Time.deltaTime;
                if (chargeTimer <= 0)
                {
                    //Fire a projectile for tech demo without animator
                    if (anim == null) ShootProjectile();

                    //Continue hovering or chasing around the player
                    //Dev note: will auto return to idle if player runs out of range
                    ariesState = AriesState.CHASE;
                    chargeTimer = chargeDuration;
                }

                //Face the player while charging a shot
                FacePlayer();

                //Reset the rigidbody
                rb.velocity = Vector2.zero;
                break;

            case AriesState.VANISH:
                //Teleport using animation event
                break;

            case AriesState.TELEPORT:
                //Teleport using animation event
                break;
            default:
                break;
        }
    }

    //Check if target is within attack or alert range
    public int TargetWithinRange(Transform target)
    {
        float distToTarget = Vector2.Distance(target.position, transform.position);
        
        if (distToTarget > alertDist) //more than 10m
        {
            //Target is outside alert range
            return 0;
        }
        else if (distToTarget > (closestDist * 2)) //6 ~ 10m
        {
            //Target is within alert range
            //Target is outside max hover range
            return 1;
        }
        else if (distToTarget > closestDist) //3 ~ 6m
        {
            //Target is within hover range
            return 2;
        }
        else //0 ~ 3m
        {
            //Target is too close
            //Move away from target
            return 3;
        }
    }

    //Chase the target Aries Ver
    //Chases in 2 dimensions and has distance restrictions
    private void ChaseTarget(Transform target, bool chaseForward)
    {
        //Move towards the player
        if (chaseForward)
        {
            //Get the direction between target and self
            Vector2 dir = target.position - transform.position;

            if (CheckGround(checkRadius * 1.5f) == true)
            {
                dir.y = 0;
            }

            //Add force towards target to move forward
            force += dir.normalized * speed * Time.deltaTime;
        }
        //Move away from the player
        else
        {
            //Reset the velocity
            rb.velocity = Vector2.zero;

            //Get the direction between target and self
            Vector2 dir = target.position - transform.position;

            //Always move upwards when moving away from target
            dir.y = -Mathf.Abs(dir.y);

            //Subtract force from target to move away
            force -= dir.normalized * speed * Time.deltaTime;
        }
    }

    //Fire a projectile at the player
    public void ShootProjectile()
    {
        //Instantiate the projectile
        GameObject projectileClone = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        //Get direction of projectile to throw at
        Vector2 dir = player.position - transform.position;

        //Get rotation angle from direction
        float projectileAngle = Mathf.Atan2(player.position.y - transform.position.y, player.position.x - transform.position.x) * Mathf.Rad2Deg;

        //Rotate projectile towards rotation angle
        projectileClone.transform.rotation = Quaternion.Euler(0, 0, projectileAngle);

        //Set normalised direction and velocity of projectile
        projectileClone.GetComponent<Rigidbody2D>().velocity = projectileClone.transform.right.normalized * projectileSpeed;


        //Apply a force away upwards
        //Add force towards target to move forward
        force += dir.normalized * speed * Time.deltaTime;
    }

    //Teleport
    private void Teleport()
    {
        //Calculate where to go and teleport


        //Change state to teleport to reappear
        ariesState = AriesState.TELEPORT;
    }

    //End teleport
    private void EndTeleport()
    {
        ariesState = AriesState.CHASE;
    }

    //Ground check
    private bool CheckGround(float groundDistance)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundDistance, groundLayer);

        if (hit.collider != null)
        {
            //print(hit.collider.name);
            return true;
        }

        return false;
    }

    //Updates the player animation
    private void UpdateAnimation()
    {
        switch (ariesState)
        {
            case AriesState.IDLE:
            case AriesState.ALERT:
            case AriesState.CHASE:
                anim.SetInteger("animState", 0);
                break;
            case AriesState.ATTACKING:
                anim.SetInteger("animState", 1);
                break;
            case AriesState.VANISH:
                anim.SetInteger("animState", 2);
                break;
            case AriesState.TELEPORT:
                anim.SetInteger("animState", 3);
                break;
            default:
                break;
        }
    }

    //Updates the player animation
    private void UpdateProceduralAnimation()
    {
        //Rotate the object based on velocity x & y
        float degRotation = rb.velocity.x * -rb.velocity.y;
        degRotation = Mathf.Clamp(degRotation, -30, 30);

        float lerpedRotation = Mathf.Lerp(transform.rotation.z, degRotation, 0.7f);
        gameObject.transform.rotation = Quaternion.Euler(0, 0, lerpedRotation);



        //Scale the object based on velocity y

        float scaleX = 1;
        float scaleY = 1;

        scaleY = Mathf.Lerp(transform.localScale.y, 1 + Mathf.Abs(rb.velocity.y) * 0.1f, 0.3f);
        scaleY = Mathf.Clamp(scaleY, 0.5f, 1.5f);
        scaleX = 2 - scaleY;

        scaleX = transform.localScale.x > 0 ? scaleX : -scaleX;
        transform.localScale = new Vector3(scaleX, scaleY, transform.localScale.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, force);
    }
}
