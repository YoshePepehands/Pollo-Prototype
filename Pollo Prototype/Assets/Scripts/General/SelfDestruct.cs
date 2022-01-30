using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    //Animation Event: Destroy the gameobject
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
