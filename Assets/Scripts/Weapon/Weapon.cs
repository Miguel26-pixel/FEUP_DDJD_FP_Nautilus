using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //private Rigidbody rigidbody;

    private void Start()
    {
        //rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter3D(Collision2D collision)
    {
        /*if (rigidbody)
        {
            Destroy(gameObject);
        }*/
    }
}
