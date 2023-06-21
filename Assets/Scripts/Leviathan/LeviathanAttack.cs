using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PlayerControls;

public class LeviathanAttack : StateMachineBehaviour
{
    GameObject player;
    const float escapeRange = 10f;
    const float attackRange = 5f;
    const int damage = 30;
    PlayerController playerController;
    const float attackDelay = 1f;
    private float nextAttackTime;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        nextAttackTime = 0f;
        playerController = player.GetComponent<PlayerController>();
        Debug.Log("Leviathan in attacking state");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.LookAt(player.transform);
        float distance = Vector3.Distance(player.transform.position, animator.transform.position);
        Debug.Log("Distance to player: " + distance.ToString());
        if (distance <= 6f && nextAttackTime < Time.time)
        {
            playerController.TakeDamage(damage);
            Debug.Log("Player attacked");
            nextAttackTime = Time.time + attackDelay;
        }
        if (distance > attackRange)
            animator.SetBool("isAttacking", false);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
