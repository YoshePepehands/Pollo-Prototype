using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolloController : MonoBehaviour
{
    //Variables
    public enum PlayerState
    {
        IDLE,
        RUN,
        CROUCH,
        SLIDE,
        JUMP,
        FALL,
        MELEE,
        RANGED,
        GUARD,
        GUARDING,
        HURT,
        DEATH
    }
    public PlayerState playerState;

    public enum PhysicalState
    {
        GROUNDED,
        ONAIR
    }
    [HideInInspector] public PhysicalState physicalState;

    //Physics Variables
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector2 force;
    private float speed = 1600;
    private float slideSpeed = 6000;
    private float maxVelocity = 5f;
    private float maxSlideVelocity = 10f;
    private float feetOffset;
    private float checkRadius = 0.2f;
    private LayerMask groundMask;
    [HideInInspector] public float jumpForce;
    private float jumpDelay = 0.2f;
    private float jumpCount = 1;

    //Animation Variables
    private BoxCollider2D boxCollider2D;
    private Vector2 originalColliderOffset;
    private Vector2 originalColliderSize;
    private Animator anim;

    //Timer Variables
    private float slideTimer;
    private float slideDuration = 0.5f;

    //Prefab Variables
    public GameObject slidingSmokePrefab;

    private PolloAbility polloAbility;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        feetOffset = gameObject.GetComponent<BoxCollider2D>().size.y / 2 * transform.localScale.y;
        groundMask = LayerMask.GetMask("Ground");

        boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        originalColliderOffset = boxCollider2D.offset;
        originalColliderSize = boxCollider2D.size;
        //Stat updates
        jumpForce = 450f;

        anim = GetComponent<Animator>();

        slideTimer = slideDuration;

        polloAbility = GetComponent<PolloAbility>();
    }

    void Update()
    {
        UpdatePlayerLogic();

        if (anim != null)
        {
            UpdateAnimation();
        }
        else
        {
            UpdateProceduralAnimation();
        }
        //print(playerState);
    }

    private void FixedUpdate()
    {
        //Dash prioritise over groundcheck
        CheckGround();

        //Prevent player from sliding
        if (force == Vector2.zero && physicalState == PhysicalState.GROUNDED)
        {
            float tempF = -rb.velocity.x * rb.mass / Time.fixedDeltaTime;
            force += new Vector2(tempF, 0);
        }

        //Prevent player from going over max velocity
        if (playerState == PlayerState.SLIDE)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSlideVelocity, maxSlideVelocity), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxVelocity, maxVelocity), rb.velocity.y);
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
            //Player is grounded
            physicalState = PhysicalState.GROUNDED;
            jumpCount = 1;
        }
        else
        {
            //Check if slide off platform
            if (playerState == PlayerState.SLIDE)
            {
                //Reset collider and offset
                SlideCollider(false);

                //Reset slide timer
                slideTimer = slideDuration;
            }

            //Player is not grounded
            physicalState = PhysicalState.ONAIR;
        }
    }

    //Update Internal Player Logic
    private void UpdatePlayerLogic()
    {
        switch (playerState)
        {
            case PlayerState.IDLE:
                Move();
                Jump();
                break;
            case PlayerState.RUN:
                Move();
                Jump();
                break;
            case PlayerState.CROUCH:
                Move();
                break;
            case PlayerState.SLIDE:
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0)
                {
                    //Check if there is head room to end slide
                    if (SlideHasRoof())
                    {
                        //No room, extend slide
                        slideTimer = slideDuration/5;
                    }
                    else
                    {
                        //Enough room, end slide
                        SlideCollider(false);
                        Move();
                        slideTimer = slideDuration;
                    }
                }
                else
                {
                    //Keep sliding
                    Slide();
                }
                break;
            case PlayerState.JUMP:
                Move();
                Jump();
                break;
            case PlayerState.FALL:
                Move();
                Jump();
                break;
            case PlayerState.MELEE:
                break;
            case PlayerState.RANGED:
                break;
            case PlayerState.GUARD:
                break;
            case PlayerState.GUARDING:
                break;
            case PlayerState.HURT:
                break;
            case PlayerState.DEATH:
                break;
            default:
                break;
        }
    }

    //Player Move Input
    private void Move()
    {
        //Horizontal Movement
        float horizontalForce = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        force += new Vector2(horizontalForce, 0);

        //Available state logic only when grounded
        if (physicalState == PhysicalState.GROUNDED)
        {
            if (horizontalForce != 0)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    playerState = PlayerState.SLIDE;
                    SpawnSmokeVFX();
                }
                else
                {
                    playerState = PlayerState.RUN;
                }

            }
            else if (Input.GetAxisRaw("Vertical") < 0)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    playerState = PlayerState.SLIDE;
                    SpawnSmokeVFX();
                }
                else
                {
                    playerState = PlayerState.CROUCH;
                }
            }
            else
            {
                playerState = PlayerState.IDLE;
            }
        }
        else if (physicalState == PhysicalState.ONAIR)
        {
            if (playerState != PlayerState.JUMP)
            {
                playerState = PlayerState.FALL;
            }
        }

        //Face the direction of movement
        FaceDirection(horizontalForce);
    }

    //Player Slide
    private void Slide()
    {
        float horizontalForce = slideSpeed * Time.deltaTime;

        if (transform.localScale.x > 0)
        {
            force += new Vector2(horizontalForce, 0);
        }
        else if (transform.localScale.x < 0)
        {
            force += new Vector2(-horizontalForce, 0);
        }
        SlideCollider(true);
    }

    //Adjust Sliding collider
    public void SlideCollider(bool isSliding)
    {
        //Player is sliding
        if (isSliding)
        {
            //Lower collider and offset
            boxCollider2D.offset = new Vector2(originalColliderOffset.x, -originalColliderSize.y / 4);
            boxCollider2D.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
        }
        //Player is finished sliding
        else
        {
            //Reset collider and offset
            boxCollider2D.offset = originalColliderOffset;
            boxCollider2D.size = originalColliderSize;
        }
    }

    //Check for roof overhead to end slide
    public bool SlideHasRoof()
    {
        //Cast a ray up to check if there is obstacle over head
        RaycastHit2D hit = Physics2D.Raycast(getFeetPos(), Vector2.up, originalColliderSize.y * transform.localScale.y, groundMask);

        if (hit.collider != null)
        {
            //There is something over head
            return true;
        }
        else
        {
            //There is nothing over head
            return false;
        }
    }

    //Spawn Sliding smoke VFX
    private void SpawnSmokeVFX()
    {
        //Instantiate sliding smoke VFX
        GameObject slidingSmokeClone = Instantiate(slidingSmokePrefab, getFeetPos(), Quaternion.identity);

        //Change effect direction
        if (transform.localScale.x < 0)
        {
            slidingSmokeClone.transform.localScale = new Vector3(-slidingSmokeClone.transform.localScale.x
                , slidingSmokeClone.transform.localScale.y
                , slidingSmokeClone.transform.localScale.z);
        }

        Destroy(slidingSmokeClone, 1f);
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
                if (physicalState == PhysicalState.GROUNDED)
                {
                    playerState = PlayerState.JUMP;
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                    force += new Vector2(0, jumpForce);
                    jumpDelay = 0.2f;
                }

                //Air Jump
                else if (physicalState == PhysicalState.ONAIR)
                {
                    if (jumpCount > 0)
                    {
                        playerState = PlayerState.JUMP;
                        jumpCount--;
                        rb.velocity = new Vector2(rb.velocity.x, 0f);
                        force += new Vector2(0, jumpForce);
                        jumpDelay = 0.2f;
                    }
                }
            }
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
        switch (playerState)
        {
            case PlayerState.IDLE:
                anim.SetInteger("animState", 0);
                break;
            case PlayerState.RUN:
                anim.SetInteger("animState", 1);
                break;
            case PlayerState.CROUCH:
                anim.SetInteger("animState", 2);
                break;
            case PlayerState.SLIDE:
                anim.SetInteger("animState", 3);
                break;
            case PlayerState.JUMP:
                anim.SetInteger("animState", 4);
                break;
            case PlayerState.FALL:
                anim.SetInteger("animState", 5);
                break;
            case PlayerState.MELEE:
                anim.SetInteger("animState", 6);
                break;
            case PlayerState.RANGED:
                break;
            case PlayerState.GUARD:
                break;
            case PlayerState.GUARDING:
                break;
            case PlayerState.HURT:
                break;
            case PlayerState.DEATH:
                break;
            default:
                break;
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

        if (physicalState == PhysicalState.GROUNDED)
        {
            //Reset to normal cube
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (physicalState == PhysicalState.ONAIR)
        {
            scaleY = Mathf.Lerp(transform.localScale.y, 1 + Mathf.Abs(rb.velocity.y) * 0.1f, 0.3f);
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
        Gizmos.DrawRay(getFeetPos(), Vector2.up * originalColliderSize.y * transform.localScale.y);
    }

}
