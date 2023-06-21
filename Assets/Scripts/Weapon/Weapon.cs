using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Rigidbody rigidbody;
    public Vector3 PickPosition;
    public Vector3 PickRotation;
    public GameObject gameObject;
    public int weaponId;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rigidbody)
        {
            //Destroy(gameObject);
        }
    }

    public void ActivateWeapon()
    {
        gameObject.SetActive(true);
    }

    public void DeactivateWeapon()
    {
        gameObject.SetActive(false);
    }
}
