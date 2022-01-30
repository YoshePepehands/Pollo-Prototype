using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (Input.GetAxisRaw("Vertical") == -1)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    StopAllCoroutines();
                    StartCoroutine(RotateEffector(180));
                }
            }
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            StopAllCoroutines();
            StartCoroutine(RotateEffector(0));
        }
    }

    private IEnumerator RotateEffector(int deg)
    {
        gameObject.GetComponent<PlatformEffector2D>().rotationalOffset = deg;
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<PlatformEffector2D>().rotationalOffset = 0;
    }
}
