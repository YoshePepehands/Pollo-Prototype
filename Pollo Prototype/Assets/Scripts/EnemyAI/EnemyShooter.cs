using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    private enum EnemyShooterState
    {
        idle,
        tracking,
        lockedOn
    }

    private EnemyShooterState enemyShooterState;

    //Variables
    private float detectionRadius = 7;     //Player detection range
    private LayerMask playerMask;           //Player's layer mask
    public GameObject projectilePrefab;     //Projectile to spawn
    private float reloadTime = 2f;          //Maximum reload time
    private float nextShotTime = 0;         //Time till next shot

    private float trackingTime = 10f;      //Maximum tracking time
    private float remainingTrackTime = 0f;   //Tracking time left to return idle

    public Transform eyes;
    public Vector2[] eyeTrackPoints;        //Places to move eye to
    private int wayPoint = 0;

    void Start()
    {
        playerMask = LayerMask.GetMask("Player");
    }

    void Update()
    {
        if (enemyShooterState == EnemyShooterState.lockedOn)
        {
            nextShotTime -= Time.deltaTime;
            if (nextShotTime <= 0)
            {
                //Shoots a bullet at the player
                Projectile projectileClone = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
                Transform player = GameObject.FindWithTag("Player").transform;
                projectileClone.forceDir = (player.position - projectileClone.transform.position).normalized;
                projectileClone.speed = 3;

                nextShotTime = reloadTime;
            }
        }
        else if (enemyShooterState == EnemyShooterState.tracking)
        {
            remainingTrackTime -= Time.deltaTime;
            if (remainingTrackTime <= 0)
            {
                enemyShooterState = EnemyShooterState.idle;
            }
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        bool hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);

        if (hit == true)
        {
            enemyShooterState = EnemyShooterState.lockedOn;
            //print("scream");
        }
        else
        {
            if (enemyShooterState == EnemyShooterState.lockedOn)
            {
                enemyShooterState = EnemyShooterState.tracking;
                remainingTrackTime = trackingTime;
                nextShotTime = reloadTime;
            }
        }
    }

    private void UpdateAnimation()
    {
        if (enemyShooterState == EnemyShooterState.tracking)
        {
            eyes.localPosition = Vector3.Lerp(eyes.localPosition, eyeTrackPoints[wayPoint], 3f * Time.deltaTime);
            if (Vector3.Distance(eyes.localPosition, eyeTrackPoints[wayPoint]) < 0.1f)
            {
                wayPoint++;
                if (wayPoint == eyeTrackPoints.Length)
                {
                    wayPoint = 0;
                }
            }
        }

        else if (enemyShooterState == EnemyShooterState.idle)
        {
            eyes.localPosition = Vector3.Lerp(eyes.localPosition, Vector3.zero, 3f * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
