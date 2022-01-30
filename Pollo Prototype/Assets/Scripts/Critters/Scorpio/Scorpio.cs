using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpio : Critters
{
    //Scorpio States
    private enum ScorpioState
    {
        IDLE,
        PATROL,
        ALERT,
        CHASE,
        LOWERING,
        SKITTERING,
        POUNCING,
        FIRING,
        RECOVERING
    }
    [SerializeField] private ScorpioState scorpioState = ScorpioState.IDLE;

    //State Timers
    private float idleTimer;
    private float idleDuration = 3f;
    private float patrolTimer;
    private float patrolDuration = 3f;
    private float alertTimer;
    private float alertDuration = 0.75f;
    private float lowerTimer;
    private float lowerDuration = 0.35f;
    private float recoverTimer;
    private float recoverDuration = 0.35f;

    //Variables
    public float alertDist = 5f;        //Distance to trigger alert
    public float attackDist = 1.5f;     //Distance to trigger attack

    private float speed = 5000f;
    private float maxVelocity = 3f;
    [SerializeField] private bool isGrounded;
    private LayerMask groundLayer;
    private float checkRadius = 0.2f;

    private int attackSelection;    //0 = pounce, 1 = shoot
    [SerializeField] private int skitterCount;       //Number of skitters during SKITTERING state
    private float skitterForce = 350f;
    private float skitterDelay = 0.2f;

    private int pounceCount = 1;
    private float pounceDelay = 0.2f;

    private Animator anim;

    [SerializeField] private GameObject projectilePrefab;
    private float projectileSpeed = 10f;

    void Start()
    {
        critterState = CritterState.ALIVE;
        idleTimer = idleDuration;
        patrolTimer = patrolDuration;
        alertTimer = alertDuration;
        lowerTimer = lowerDuration;
        recoverTimer = recoverDuration;

        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateCritterLogic();
        

        if (Input.GetKeyDown(KeyCode.G))
        {
            TakeDamage(1, true);
        }
        //print(force);

        if (anim != null)
        {
            UpdateAnimation();
        }
        else
        {
            UpdateProceduralAnimation();
        }
        
        
    }

    private void FixedUpdate()
    {
        //Check if scorpio is grounded
        CheckGround();

        //Prevent scorpio from sliding
        //if (force == Vector2.zero && isGrounded)
        //{
        //    float tempF = (-rb.velocity.x * rb.mass) / Time.fixedDeltaTime;
        //    force += new Vector2(tempF, 0);
        //}

        //Prevent scorpio from going over max velocity
        //if (Mathf.Abs(rb.velocity.x) > maxVelocity)
        //{
        //    if (scorpioState == ScorpioState.SKITTERING) return;
            
            
        //    //if (rb.velocity.x < 0)
        //    //{
        //    //    rb.velocity = new Vector2(-maxVelocity, rb.velocity.y);
        //    //}
        //    //else
        //    //{
        //    //    rb.velocity = new Vector2(maxVelocity, rb.velocity.y);
        //    //}
        //}

        
        //Add final force to rigidbody
        rb.AddForce(force);
        force = Vector2.zero;

        //Clamp Exceptions
        if (scorpioState == ScorpioState.POUNCING)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxVelocity * 1.2f, maxVelocity * 1.2f), rb.velocity.y);
        }
        else if (scorpioState == ScorpioState.SKITTERING && attackSelection == 0)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxVelocity * 1.5f, maxVelocity * 1.5f), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxVelocity, maxVelocity), rb.velocity.y);
        }

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
                scorpioState = ScorpioState.ALERT;
            }

            //Goes into alert
            //scorpioState = ScorpioState.ALERT;
        }

        //Update Scorpio logic
        if (critterState == CritterState.ALIVE)
        {
            UpdateScorpioLogic();
        }
    }

    //Scorpio internal behaviour loop
    public void UpdateScorpioLogic()
    {
        switch (scorpioState)
        {
            case ScorpioState.IDLE:
                //Count down idle timer and switch to patrol
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    scorpioState = ScorpioState.PATROL;
                    idleTimer = idleDuration;
                }
                //Check if player is within range switch to alert
                if (TargetWithinRange(player) != 0)
                {
                    scorpioState = ScorpioState.ALERT;
                    idleTimer = idleDuration;
                }

                break;
            case ScorpioState.PATROL:

                //Face direction of patrol
                FaceDirection();

                //Count down patrol timer and switch to idle
                patrolTimer -= Time.deltaTime;
                if (patrolTimer <= 0)
                {
                    scorpioState = ScorpioState.IDLE;
                    patrolTimer = patrolDuration;
                }
                //Check if player is within range switch to alert
                if (TargetWithinRange(player) != 0)
                {
                    scorpioState = ScorpioState.ALERT;
                    patrolTimer = patrolDuration;
                }

                break;
            case ScorpioState.ALERT:
                //Count down alert timer and switch to chase
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0)
                {
                    switch (TargetWithinRange(player))
                    {
                        case 0: //Player has gotten too far away
                            scorpioState = ScorpioState.PATROL;
                            break;
                        case 1: //Player is within chase distance
                            scorpioState = ScorpioState.CHASE;
                            break;
                        case 2: //Player is within attack distance
                            scorpioState = ScorpioState.LOWERING;
                            break;
                        default: //Should not enter default
                            scorpioState = ScorpioState.PATROL;
                            break;
                    }
                    alertTimer = alertDuration;
                }

                //Face the player when lowered
                FacePlayer();
                break;
            case ScorpioState.CHASE:
                //Face direction of chase
                FaceDirection();

                //Chase the player
                switch (TargetWithinRange(player))
                {
                    case 0: //Player has gotten too far away
                        scorpioState = ScorpioState.PATROL;
                        break;
                    case 1: //Player is within chase distance
                        //Chase the player
                        //Player is on the left
                        if (player.transform.position.x < transform.position.x)
                        {
                            //Add speed left to force
                            force += new Vector2(-speed, 0) * Time.deltaTime;
                        }
                        //Player is on the right
                        else
                        {
                            //Add speed right to force
                            force += new Vector2(speed, 0) * Time.deltaTime;
                        }
                        break;
                    case 2: //Player is within attack distance
                        scorpioState = ScorpioState.LOWERING;
                        break;
                    default: //Should not enter default
                        scorpioState = ScorpioState.PATROL;
                        break;
                }
                //Check if player is within close range switch to lower or skitter
                //Check if player is within range switch to patrol if not
                break;
            case ScorpioState.LOWERING:
                //Count down lower timer and switch to skiterring 1 or 3
                lowerTimer -= Time.deltaTime;
                if (lowerTimer <= 0)
                {
                    //Randomly pick between pouncing or shooting at the player
                    attackSelection = Random.Range(0, 2);

                    //Temp hardcode to test
                    //attackSelection = 1;

                    if (attackSelection == 0)
                    {
                        skitterCount = 1;
                    }
                    else if (attackSelection == 1)
                    {
                        skitterCount = 3;
                    }
                    
                    scorpioState = ScorpioState.SKITTERING;
                    lowerTimer = lowerDuration;
                }

                //Face the player when lowered
                FacePlayer();

                break;
            case ScorpioState.SKITTERING:
                //Check if scorpio is grounded
                if (isGrounded)
                {
                    Skitter();
                    
                }

                //Switch to pounce after skitter 1 and firing after skitter 3
                //Get target position before switching to pounce
                //Get target position before switching to firing
                break;
            case ScorpioState.POUNCING:
                //Curve pounce and switch to alert after landing
                //Check if scorpio is grounded
                if (isGrounded)
                {
                    Pounce();
                }
                
                break;
            case ScorpioState.FIRING:
                //Jump up and fire 3 spikes and switch to alert after landing
                scorpioState = ScorpioState.ALERT;
                break;
            case ScorpioState.RECOVERING:
                //Count down before switching back to chase
                recoverTimer -= Time.deltaTime;
                if (recoverTimer <= 0)
                {
                    //Chase the player
                    switch (TargetWithinRange(player))
                    {
                        case 0: //Player has gotten too far away
                            scorpioState = ScorpioState.PATROL;
                            break;
                        case 1: //Player is within chase distance
                            scorpioState = ScorpioState.CHASE;
                            break;
                        case 2: //Player is within attack distance
                            scorpioState = ScorpioState.LOWERING;
                            break;
                        default: //Should not enter default
                            scorpioState = ScorpioState.PATROL;
                            break;
                    }
                    recoverTimer = recoverDuration;
                }
                break;
            default:
                break;
        }
    }

    //Check if target is within attack or alert range
    public int TargetWithinRange(Transform target)
    {
        float distToTarget = Vector2.Distance(target.position, transform.position);
        if (distToTarget < attackDist)
        {
            //Target is within attack range
            return 2;
        }
        else if (distToTarget < alertDist)
        {
            //Target is within alert range
            return 1;
        }
        else
        {
            //Target is outside alert range
            return 0;
        }
    }

    //Skitter
    private void Skitter()
    {
        skitterDelay -= Time.deltaTime;
        if (skitterDelay <= 0)
        {
            skitterDelay = 0;

            if (skitterCount > 0)
            {
                //Player is on the left
                if (player.transform.position.x < transform.position.x)
                {
                    //Skitter for pounce
                    if (attackSelection == 0)
                    {
                        //Skitter to the right
                        force += new Vector2(skitterForce * 2f, skitterForce);
                    }
                    //Skitter for firing
                    else if (attackSelection == 1)
                    {
                        //Skitter to the right
                        force += new Vector2(skitterForce, skitterCount == 1 ? skitterForce * 1.8f : skitterForce);
                        if (skitterCount == 1)
                        {
                            Invoke("ShootProjectile", 0.67f);
                        }
                    }
                }
                //Player is on the right
                else
                {
                    //Skitter for pounce
                    if (attackSelection == 0)
                    {
                        //Skitter to the left
                        force += new Vector2(-skitterForce * 2f, skitterForce);
                    }
                    //Skitter for firing
                    else if (attackSelection == 1)
                    {
                        //Skitter to the left
                        force += new Vector2(-skitterForce, skitterCount == 1 ? skitterForce * 1.8f : skitterForce);
                        if (skitterCount == 1)
                        {
                            Invoke("ShootProjectile", 0.67f);
                        }
                    }
                }
                
                skitterCount--;

                skitterDelay = 0.2f;
                
                //print("skittering");
            }
            //Switch to pounce or firing state
            else
            {
                if (attackSelection == 0)
                {
                    scorpioState = ScorpioState.POUNCING;
                }
                else if (attackSelection == 1)
                {
                    scorpioState = ScorpioState.FIRING;
                }
            }

            FacePlayer(); //Face the player when skittering back or pouncing forward
        }
    }

    //Pounce
    private void Pounce()
    {
        pounceDelay -= Time.deltaTime;
        if (pounceDelay <= 0)
        {
            if (pounceCount > 0)
            {
                //Check player direction
                //Player is on the left
                if (player.transform.position.x < transform.position.x)
                {
                    //Add speed left to force
                    force += new Vector2(-600f, 700f);
                }
                //Player is on the right
                else
                {
                    //Add speed right to force
                    force += new Vector2(600f, 700f);
                }
                pounceCount--;
                pounceDelay = 0.2f;
            }
            else
            {
                scorpioState = ScorpioState.ALERT;
                pounceCount = 1;
            }
        }
        
    }

    //Fire a projectile at the player
    public void ShootProjectile()
    {
        int noOfProjectiles = 3;
        for (int i = 0; i < noOfProjectiles; i++)
        {
            //Instantiate the projectile
            GameObject projectileClone = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            //Get direction of projectile to throw at
            Vector2 dir = player.position - transform.position;

            //Rotate projectile towards direction
            float projectileAngle = Mathf.Atan2(player.position.y - transform.position.y, player.position.x - transform.position.x) * Mathf.Rad2Deg;

            //For fun
            if (i != 0)
            {
                if (i % 2 == 0)
                {
                    projectileAngle += i * 5f;
                    dir.y -= i * 0.5f;
                }
                else
                {
                    projectileAngle -= i * 5f;
                    dir.y += i * 0.5f;
                }
            }
            
            projectileClone.transform.rotation = Quaternion.Euler(0, 0, projectileAngle);

            //Set normalised direction and velocity of projectile
            projectileClone.GetComponent<Rigidbody2D>().velocity = dir.normalized * projectileSpeed;

        }
    }

    //Calculate grounded offset position
    private Vector2 getFeetPos()
    {
        return transform.position - new Vector3(0, GetComponent<CircleCollider2D>().radius, 0);
    }

    //Check if scorpio is grounded
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.CircleCast(getFeetPos(), checkRadius, Vector2.right, 0, groundLayer);

        if (hit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    //Updates the player animation
    private void UpdateProceduralAnimation()
    {
        ////Squash & Stretch
        //const float s1 = 0.2f;
        //const float s2 = 0.2f;
        //var v = new Vector2(
        //    1 - acceleration.x * s1 + acceleration.y * s2
        //  , 1 - acceleration.y * s2 + acceleration.x * s1);

        ////Decay acceleration
        //acceleration *= 0.9f;


        //Rotate the object based on velocity x & y
        float degRotation = rb.velocity.x * -rb.velocity.y;
        degRotation = Mathf.Clamp(degRotation, -30, 30);

        float lerpedRotation = Mathf.Lerp(transform.rotation.z, degRotation, 0.7f);
        gameObject.transform.rotation = Quaternion.Euler(0, 0, lerpedRotation);



        //Scale the object based on velocity y

        float scaleX = 1;
        float scaleY = 1;

        if (isGrounded)
        {
            //Reset to normal cube
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            scaleY = Mathf.Lerp(transform.localScale.y, 1 + Mathf.Abs(rb.velocity.y) * 0.1f, 0.3f);
            scaleY = Mathf.Clamp(scaleY, 0.5f, 1.5f);
            scaleX = 2 - scaleY;
        }
        scaleX = transform.localScale.x > 0 ? scaleX : -scaleX;
        transform.localScale = new Vector3(scaleX, scaleY, transform.localScale.z);
    }

    //Updates the player animation
    private void UpdateAnimation()
    {
        switch (scorpioState)
        {
            case ScorpioState.IDLE:
                anim.SetInteger("animState", 0);
                break;
            case ScorpioState.PATROL:
                anim.SetInteger("animState", 1);
                break;
            case ScorpioState.ALERT:
                anim.SetInteger("animState", 2);
                break;
            case ScorpioState.CHASE:
                anim.SetInteger("animState", 1);
                break;
            case ScorpioState.LOWERING:
                anim.SetInteger("animState", 2);
                break;
            case ScorpioState.SKITTERING:
                if (isGrounded)
                {
                    anim.SetInteger("animState", 0);
                }
                else
                {
                    anim.SetInteger("animState", 3);
                }
                break;
            case ScorpioState.POUNCING:
                anim.SetInteger("animState", 3);
                break;
            case ScorpioState.FIRING:
                if (isGrounded)
                {
                    anim.SetInteger("animState", 0);
                }
                else
                {
                    anim.SetInteger("animState", 3);
                }
                break;
            case ScorpioState.RECOVERING:
                anim.SetInteger("animState", 0);
                break;
            default:
                break;
        }
    }

    //Debug Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(getFeetPos(), checkRadius);
    }
}
