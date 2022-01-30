using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbility : MonoBehaviour
{
    //Variables
    public GameObject fireballPrefab;
    private float chargeTimer = 0;
    private float fireballCooldown = 1f;

    void Start()
    {

    }

    void Update()
    {
        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0)
        {
            chargeTimer = 0;
            KeyboardFireball();
            MouseFireball();
        }
        
    }

    private void KeyboardFireball()
    {
        if (Input.GetKey(KeyCode.E))
        {
            //Get player controller script
            PlayerController playerController = gameObject.GetComponent<PlayerController>();

            //Instantiate fireball and give it speed
            Projectile fireballClone = Instantiate(fireballPrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
            fireballClone.speed = 8;

            //Give fireball direction and rotation
            if (Input.GetKey(KeyCode.UpArrow))
            {
                fireballClone.forceDir = new Vector2(0, 1);
                fireballClone.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                fireballClone.forceDir = new Vector2(0, -1);
                fireballClone.transform.rotation = Quaternion.Euler(0, 0, 270);
            }
            else if (transform.localScale.x > 0)
            {
                fireballClone.forceDir = new Vector2(1, 0);
                fireballClone.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                fireballClone.forceDir = new Vector2(-1, 0);
                fireballClone.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            //Knockback player opposite direction of fireball
            if (playerController.playerState == PlayerController.PlayerState.air)
            {
                playerController.rb.velocity = new Vector2(playerController.rb.velocity.x, 0);
                playerController.force += -fireballClone.forceDir * playerController.jumpForce * 1.5f;
            }

            chargeTimer = fireballCooldown;
        }
    }

    private void MouseFireball()
    {
        if (Input.GetMouseButton(0))
        {
            //Get mouse position and convert to vector for fireball direction
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            //Instantiate fireball and give it direction and speed
            Projectile fireballClone = Instantiate(fireballPrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
            fireballClone.forceDir = (mousePos - transform.position).normalized;
            fireballClone.speed = 8;

            //Rotate fireball towards mouse position
            float fireballAngle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg;
            fireballClone.transform.rotation = Quaternion.Euler(0, 0, fireballAngle);

            //Knockback player opposite direction of fireball
            PlayerController playerController = gameObject.GetComponent<PlayerController>();
            if (playerController.playerState == PlayerController.PlayerState.air)
            {
                playerController.rb.velocity = new Vector2(playerController.rb.velocity.x, 0);
                playerController.force += -fireballClone.forceDir * playerController.jumpForce * 1.5f;
            }

            chargeTimer = fireballCooldown;
        }
    }
}
