using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private Weapon currentWeapon;

    [SerializeField] private Shooter shooter;

    [SerializeField]
    private float spawnDuration = 2;

    [SerializeField]
    private Player player;

    void Start()
    {

    }

    public void Fire(Vector3 position)
    {
        Vector3 screenPos = position;
        Vector3 shootingDirection = screenPos.normalized;
        GameObject weapon =  Instantiate(currentWeapon.gameObject, shooter.transform.position, Quaternion.identity);
        weapon.GetComponent<Rigidbody>().velocity = new Vector3(shootingDirection.x, shootingDirection.y, shootingDirection.z);
    }

}
