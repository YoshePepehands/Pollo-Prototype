using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        grounded,
        air,
        dash
    }
    [HideInInspector] public PlayerState playerState;

    //Physics Variables
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector2 force;
    public float speed = 1600;
    private float maxVelocity = 5f;
    private float feetOffset;
    private float checkRadius = 0.2f;
    private LayerMask groundMask;
    [HideInInspector] public float jumpForce;
    private float jumpDelay = 0.2f;
    private float jumpCount = 1;

    //Dash Variables
    private float dashDuration = 0.3f;
    private float dashTimer = 0;
    private float dashSpeed = 800;

    //Animation Variables
    private CapsuleCollider2D capsuleCollider2D;
    private Vector3 originalSize;
    private Vector3 originalScale;
    private Vector2 acceleration;


    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        feetOffset = gameObject.GetComponent<CapsuleCollider2D>().size.y / 2;
        groundMask = LayerMask.GetMask("Ground");

        capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
        originalSize = capsuleCollider2D.size;
        originalScale = transform.localScale;

        //Stat updates
        jumpForce = 450f;
    }

    void Update()
    {
        
        Dash();

        //Dash prioritise over movement and jump
        if (dashTimer <= 0)
        {
            //Horizontal Movement
            float horizontalForce = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
            force += new Vector2(horizontalForce, 0);

            Jump();
            FaceDirection(horizontalForce);
        }
        
        UpdateAnimation();

    }

    private void FixedUpdate()
    {
        //Dash prioritise over groundcheck
        if (dashTimer <= 0)
        {
            CheckGround();
        }

        //Prevent player from sliding
        if (force == Vector2.zero && playerState == PlayerState.grounded)
        {
            float tempF = -rb.velocity.x * rb.mass / Time.fixedDeltaTime;
            force += new Vector2(tempF, 0);
        }

        //Prevent player from going over max velocity
        if (Mathf.Abs(rb.velocity.x) > maxVelocity && dashTimer <= 0)
        {
            if (rb.velocity.x < 0)
            {
                rb.velocity = new Vector2(-maxVelocity, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(maxVelocity, rb.velocity.y);
            }
            
        }

        rb.AddForce(force);
        force = Vector2.zero;
    }

    //Calculate grounded offset position
    private Vector2 getFeetPos()
    {
        return transform.position - new Vector3(0, feetOffset, 0);
    }

    //Check if player is grounded
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.CircleCast(getFeetPos(), checkRadius, Vector2.right, 0, groundMask);

        if (hit.collider != null)
        {
            playerState = PlayerState.grounded;
            jumpCount = 1;
        }
        else
        {
            playerState = PlayerState.air;
        }
    }

    //Player Jump Input
    private void Jump()
    {
        jumpDelay -= Time.deltaTime;
        if (jumpDelay <= 0)
        {
            jumpDelay = 0;

            if (Input.GetKeyDown(KeyCode.Space) && Input.GetAxisRaw("Vertical") != -1)
            {
                //Ground Jump
                if (playerState == PlayerState.grounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                    force += new Vector2(0, jumpForce);
                    jumpDelay = 0.2f;
                }

                //Air Jump
                else if (playerState == PlayerState.air)
                {
                    if (jumpCount > 0)
                    {
                        jumpCount--;
                        rb.velocity = new Vector2(rb.velocity.x, 0f);
                        force += new Vector2(0, jumpForce);
                        jumpDelay = 0.2f;
                    }
                }
            }
        }
    }

    //Player Dash Input
    private void Dash()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0)
        {
            dashTimer = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Q))
            {
                dashTimer = dashDuration;

                if (transform.localScale.x > 0)
                {
                    force += new Vector2(dashSpeed, 0);
                }
                else
                {
                    force += new Vector2(-dashSpeed, 0);
                }
            }
        }
        else
        {
            playerState = PlayerState.dash;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

            
        }
    }

    //Player face proper input direction
    private void FaceDirection(float horizontalForce)
    {
        if (horizontalForce < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1, transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalForce > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    //Updates the player animation
    private void UpdateAnimation()
    {
        //if (playerState == PlayerState.grounded)
        //{
        //    //Player idle or moving animation
        //}
        //else if (playerState == PlayerState.air)
        //{
        //    //Player jump or falling animation
        //}

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

        if (playerState == PlayerState.grounded)
        {
            //Reset to normal cube
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (playerState == PlayerState.air)
        {
            scaleY = Mathf.Lerp(transform.localScale.y, 1 + Mathf.Abs(rb.velocity.y)*0.1f, 0.3f);
            scaleY = Mathf.Clamp(scaleY, 0.5f, 1.5f);
            scaleX = 2 - scaleY;
        }
        scaleX = transform.localScale.x > 0 ? scaleX : -scaleX;
        transform.localScale = new Vector3(scaleX, scaleY, transform.localScale.z);
    }

    //Debug Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(getFeetPos(), checkRadius);
    }
}
