using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeviathanChase : StateMachineBehaviour
{
    Transform player;
    const float chasingSpeed = 2f;
    const float escapeRange = 25f;
    const float attackRange = 5f;
    public LayerMask waterLayer;
    private GameObject playerComplete;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerComplete = GameObject.FindGameObjectWithTag("Player");
        waterLayer = LayerMask.GetMask("Water");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (IsPlayerInChaseRange(animator))
        {
            MoveTowardsTargetPosition(animator);
            float distance = Vector3.Distance(player.position, animator.transform.position);
            if (distance > escapeRange)
                animator.SetBool("isChasing", false);
            else if (distance <= attackRange)
                animator.SetBool("isAttacking", true);
        }
        else
        {
            animator.SetBool("isChasing", false);
            Debug.Log("Leviathan not in range anymore");
        }
    }

    private bool IsPlayerInChaseRange(Animator animator)
    {
        if (playerComplete.transform.position.y <= 21f)
            return true;
        else
            return false;
    }

    private void MoveTowardsTargetPosition(Animator animator)
    {
        Vector3 direction = (player.position - animator.transform.position);
        direction.Normalize();
        // Implement movement logic here, such as using Translate, Rigidbody, or other movement methods
        // Update the enemy's position based on the desired movement direction and speed
        Vector3 movement = direction * chasingSpeed * Time.deltaTime;
        // Calculate the rotation to face the target position
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        //targetRotation *= Quaternion.Euler(0f, 180f, 0f);
        animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, targetRotation, Time.deltaTime);
        animator.transform.Translate(movement, Space.World);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //agent.SetDestination(animator.transform.position);
    }

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
