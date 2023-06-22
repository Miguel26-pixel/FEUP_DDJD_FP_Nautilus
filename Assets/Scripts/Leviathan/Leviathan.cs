using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leviathan : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;
    public Slider healthBar;
    public Animator animator;

    public void Update()
    {
        healthBar.value = (health/maxHealth)*100;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            //Play death animation
            gameObject.SetActive(false);
            GetComponent<Collider>().enabled = false;
            // FindObjectOfType<AudioManager>().Play("LeviathanDeath");
            animator.SetTrigger("die");
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Weapon")) {
            TakeDamage(20);
        }
    }
}
