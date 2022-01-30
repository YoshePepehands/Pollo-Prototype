using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflectorAbility : MonoBehaviour
{
    //Variables
    private GameObject player;
    private GameObject target;
    private float quickDeflect = 0.25f;
    private float holdTime = 0;
    private bool isDeflecting = false;
    public GameObject arrow;
    float angleDeg;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        //Increase deflecting time to differentiate quick deflect or bash
        if (isDeflecting)
        {
            holdTime += Time.deltaTime;
        }

        //Brings up the deflector if clicked
        if (Input.GetMouseButtonDown(1))
        {
            gameObject.GetComponent<Animator>().SetBool("isDetecting", true);
            isDeflecting = true;
            holdTime = 0;
            target = null;
        }
        //Closes the deflector if released
        else if (Input.GetMouseButtonUp(1))
        {
            //Bash ability activated
            if (target != null)
            {
                //Get mouse position and convert to vector for bash direction
                Vector3 mousePosi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosi.z = 0;
                mousePosi.y -= 2;
                Vector2 bashDir = (mousePosi - target.transform.position).normalized;

                //Player flies towards mouse direction if not grounded
                if (player.GetComponent<PlayerController>().playerState != PlayerController.PlayerState.grounded)
                {
                    player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    player.GetComponent<Rigidbody2D>().velocity = bashDir * 10;
                }

                //Target fly in opposite mouse direction
                target.GetComponent<Rigidbody2D>().velocity = -bashDir * 10;

            }

            gameObject.GetComponent<Animator>().SetBool("isDetecting", false);
            isDeflecting = false;
            target = null;
            Time.timeScale = 1;
        }

        //Deflector moves to player for deflect
        if (target == null)
        {
            transform.position = player.transform.position;
            arrow.SetActive(false);
            Time.timeScale = 1;
        }
        //Deflector moves to target object for bash
        else
        {
            transform.position = player.transform.position;
            //transform.position = Vector3.Lerp(transform.position, target.transform.position, 0.05f);
            arrow.SetActive(true);
            Time.timeScale = 0.15f;
        }

        //Rotate deflector towards mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        mousePos.y -= 2;

        angleDeg = Mathf.Atan2(mousePos.y - transform.position.y
            , mousePos.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angleDeg);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Collision is a projectile
        if (collision.tag == "Projectile" && collision.GetComponent<Projectile>().playerShot == false)
        {
            //Quickly deflect the projectile if click was instantaneous
            if (holdTime < quickDeflect)
            {
                collision.GetComponent<Projectile>().playerShot = true;
                collision.GetComponent<Rigidbody2D>().velocity *= -2;
            }
            //Allows the player to redirect the projectile and let player bash off it
            else
            {
                if (target == null && isDeflecting == true)
                {
                    target = collision.gameObject;
                    collision.GetComponent<Projectile>().playerShot = true;
                }
                
            }
        }
        //Allows the player to bash off an enemy or obstacle
        else if (collision.tag == "Enemy" || collision.tag == "Boulder")
        {
            if (target == null && isDeflecting == true)
            {
                target = collision.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Remove bash if player moves too far away from target
        if (collision.gameObject == target)
        {
            target = null;
            gameObject.GetComponent<Animator>().SetBool("isDetecting", false);
            isDeflecting = false;
        }
    }
}
