using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    //Variables
    public Rigidbody2D hook;
    public GameObject linkPrefab;
    public int links = 5;
    public GameObject linkedObject;

    void Start()
    {
        GenerateRope();
    }

    private void GenerateRope()
    {
        Rigidbody2D prevRb = hook;
        for (int i = 0; i < links; i++)
        {
            GameObject link = Instantiate(linkPrefab, transform);
            HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
            joint.connectedBody = prevRb;

            prevRb = link.GetComponent<Rigidbody2D>();
            if (i == links - 1 && linkedObject != null)
            {
                joint = linkedObject.AddComponent<HingeJoint2D>();
                //joint = linkedObject.GetComponent<HingeJoint2D>();
                joint.connectedBody = prevRb;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector2.zero;
                joint.connectedAnchor = new Vector2(0, -(joint.transform.localScale.x + 2));
            }
        }
    }
}
