using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leviathan : MonoBehaviour
{
    private int health = 100;
    public Animator animator;


    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            //Play death animation
            animator.SetTrigger("die");
        }
        else
        {
            //Play get hit animation
            animator.SetTrigger("damage");
        }
    }
}
