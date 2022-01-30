using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Libra : Critters
{
    //Libra States
    private enum LibraState
    {
        IDLE,
        ALERT,
        TARGETTING,
        CHARGING,
        FIRING
    }
    [SerializeField] private LibraState libraState = LibraState.IDLE;

    //State Timers
    private float idleTimer;
    private float idleDuration = 3f;
    private float alertTimer;
    private float alertDuration = 0.75f;
    private float targetTimer;
    private float targetDuration = 1.75f;
    private float chargeTimer;
    private float chargeDuration = 0.75f;
    private float firingTimer;
    private float firingDuration = 2f;

    //Variables
    private float alertDist = 7f;       //Distance to trigger alert
    public Vector3 targetLock;          //Lock on position to fire
    public Transform libraEyes;         //Eyes of the libra for track display
    private LineRenderer lineRend;      //Line renderer from the eyes

    private LayerMask groundLayer;      //Laser casts until ground layer

    void Start()
    {
        critterState = CritterState.ALIVE;
        idleTimer = idleDuration;
        alertTimer = alertDuration;
        targetTimer = targetDuration;
        chargeTimer = chargeDuration;
        firingTimer = firingDuration;

        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
        lineRend = libraEyes.GetComponent<LineRenderer>();
    }

    void Update()
    {
        UpdateCritterLogic();
    }

    //Critter internal behaviour loop
    public void UpdateCritterLogic()
    {
        //Dev Note: Testing design choice where Libra should
        // be inanimate and cannot be hurt

        //Update Libra logic
        if (critterState == CritterState.ALIVE)
        {
            UpdateLibraLogic();
        }
    }

    //Libra internal behaviour loop
    public void UpdateLibraLogic()
    {
        switch (libraState)
        {
            case LibraState.IDLE:
                //Cooldown timer prevent spam attack
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    //Check if player is within range switch to alert
                    if (TargetWithinRange(player))
                    {
                        libraState = LibraState.ALERT;
                    }
                }

                //Close eyes during idle
                libraEyes.localScale = new Vector3(libraEyes.localScale.x, Mathf.Lerp(libraEyes.localScale.y, 0, Time.deltaTime * 5), libraEyes.localScale.z);

                break;
            case LibraState.ALERT:
                //Count down alert timer and switch to targetting
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0)
                {
                    libraState = LibraState.TARGETTING;
                    alertTimer = alertDuration;
                }

                //Close eyes
                libraEyes.localScale = new Vector3(libraEyes.localScale.x, Mathf.Lerp(libraEyes.localScale.y, 1, Time.deltaTime * 5), libraEyes.localScale.z);

                //Attempt to look at player
                EyeTargetTrack(player);
                break;
            case LibraState.TARGETTING:

                //Track the player for this duration
                targetTimer -= Time.deltaTime;
                if (targetTimer <= 0)
                {
                    libraState = LibraState.CHARGING;
                    targetTimer = targetDuration;
                }
                else
                {
                    //Attempt to lock on to player position
                    targetLock = player.position;
                }

                //Attempt to look at player
                EyeTargetTrack(player);

                //Enable target line indicator
                lineRend.enabled = true;
                //Display targetting line
                DisplayTargetLine();

                ////Check if player is within range to attack
                //if (TargetWithinRange(player))
                //{
                //    //Track the player for this duration
                //    targetTimer -= Time.deltaTime;
                //    if (targetTimer <= 0)
                //    {
                //        libraState = LibraState.CHARGING;
                //        targetTimer = targetDuration;
                //    }
                //    else
                //    {
                //        //Attempt to lock on to player position
                //        targetLock = player.position;
                //    }

                //    //Attempt to look at player
                //    EyeTargetTrack(player);

                //    //Enable target line indicator
                //    lineRend.enabled = true;
                //    //Display targetting line
                //    DisplayTargetLine();
                //}
                ////Player is outside of range
                //else
                //{
                //    //Reset timer and return to idle
                //    libraState = LibraState.IDLE;
                //    targetTimer = targetDuration;

                //    //Disable target line indicator
                //    lineRend.enabled = false;
                //}

                break;
            case LibraState.CHARGING:
                chargeTimer -= Time.deltaTime;
                if (chargeTimer <= 0)
                {
                    libraState = LibraState.FIRING;
                    chargeTimer = chargeDuration;

                    //Disable target line indicator
                    //lineRend.enabled = false;
                }

                break;
            case LibraState.FIRING:
                //Fire at target spot for x amt of time
                firingTimer -= Time.deltaTime;
                //End firing
                if (firingTimer <= 0)
                {
                    //Reset to idle for cooldown
                    idleTimer = idleDuration;
                    libraState = LibraState.IDLE;
                    firingTimer = firingDuration;

                    //TEMP
                    //Disable target line indicator
                    lineRend.enabled = false;
                }
                //Cast a laser
                else
                {

                }
                break;
            default:
                break;
        }
    }

    //Check if target is within alert range
    public bool TargetWithinRange(Transform target)
    {
        float distToTarget = Mathf.Abs(target.position.x - transform.position.x);
        if (distToTarget < alertDist)
        {
            //Target is within alert range
            return true;
        }
        else
        {
            return false;
        }
    }

    //Eye tracking
    private void EyeTargetTrack(Transform target)
    {
        //Get distance between player and eyes x pos
        float distToTarget = target.position.x - libraEyes.position.x;
        //Clamp realistic value within size of head
        distToTarget = Mathf.Clamp(distToTarget, -0.1f, 0.1f);

        //Set position of eyes with x to lerp
        libraEyes.localPosition = new Vector3(Mathf.Lerp(libraEyes.localPosition.x, distToTarget, Time.deltaTime * 2f), libraEyes.localPosition.y, libraEyes.position.z);
    }

    //Display a line to targetLock
    private void DisplayTargetLine()
    {
        Vector3 dir = targetLock - libraEyes.position;
        RaycastHit2D hit = Physics2D.Raycast(libraEyes.position, dir, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            lineRend.SetPosition(0, libraEyes.position);
            lineRend.SetPosition(1, hit.point);
            //Debug.DrawLine(libraEyes.position, hit.point, Color.red);
            //print("drawing a line");
        }
        
    }

}
