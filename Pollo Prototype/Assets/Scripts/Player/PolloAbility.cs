using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolloAbility : MonoBehaviour
{
    //Variables
    private PolloController polloController;
    private float skillCooldown = 0.3f;
    private float timeToNextUse = 0;

    public bool isMeleeAttacking = false;

    //Physics Variables
    private Vector2 hitColliderSize;
    private LayerMask enemyLayer;

    //Stat Variables
    private int damageAmount;

    void Start()
    {
        polloController = GetComponent<PolloController>();
        timeToNextUse = skillCooldown;
        enemyLayer = LayerMask.GetMask("Enemy");
        hitColliderSize = new Vector2(2f, 2f);
    }

    void Update()
    {
        timeToNextUse -= Time.deltaTime;
        if (timeToNextUse <= 0)
        {
            timeToNextUse = 0;

            //Left mouse button down
            if (Input.GetKeyDown(KeyCode.X) || Input.GetMouseButtonDown(0))
            {
                //Check if player is not in the air, hurt, or dead
                if (polloController.physicalState != PolloController.PhysicalState.ONAIR
                && polloController.playerState != PolloController.PlayerState.HURT
                && polloController.playerState != PolloController.PlayerState.DEATH)
                {
                    //Check if pollo has roof to stand and change state
                    if (polloController.SlideHasRoof() == false)
                    {
                        //Reset player collider
                        polloController.SlideCollider(false);

                        //Change state to melee attack
                        polloController.playerState = PolloController.PlayerState.MELEE;

                        //Play melee attack sfx
                        //FindObjectOfType<AudioManager>().Play("PlayerSwing");

                        //Reset melee attack cooldown
                        timeToNextUse = skillCooldown;
                    }
                    else
                    {
                        //No space to stand
                        //print("no space");
                    }
                }
            }
        }

        //Hotfix
        if (polloController.physicalState == PolloController.PhysicalState.ONAIR)
        {
            DisableAttack();
        }
    }

    private void FixedUpdate()
    {
        if (isMeleeAttacking)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(GetAttackPos(), hitColliderSize, 0, Vector2.right, 0, enemyLayer);

            for (int i = 0; i < hits.Length; i++)
            {
                hits[i].collider.GetComponent<Critters>().TakeDamage(damageAmount, true);
            }
        }

    }

    private Vector2 GetAttackPos()
    {
        if (transform.localScale.x > 0)
        {
            return transform.position + new Vector3(1f, 0);
        }
        else
        {
            return transform.position + new Vector3(-1f, 0);
        }
    }

    //Animation Event: Enable attack
    public void EnableAttack()
    {
        isMeleeAttacking = true;    //Enables hitbox for enemy to take damage
    }

    //Animation Event: Disable attack
    public void DisableAttack()
    {
        isMeleeAttacking = false;   //Disables hitbox for enemy to take damage

        //Set player back to grounded if attack animation ended normally
        //Don't need to set state if player took damage in the middle of attack animation, state is changed by TakeDamage function
        if (polloController.playerState != PolloController.PlayerState.HURT && polloController.playerState != PolloController.PlayerState.DEATH)
        {
            polloController.playerState = PolloController.PlayerState.IDLE;
        }
    }

    private void OnDrawGizmos()
    {
        if (isMeleeAttacking) Gizmos.DrawWireCube(GetAttackPos(), hitColliderSize);
    }
}
