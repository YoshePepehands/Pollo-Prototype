using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yoshef : MonoBehaviour
{
    public static Vector3 GetMousePos()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
}
