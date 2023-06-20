using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeviathanPatrol : StateMachineBehaviour
{
    float timer;
    const int patrolTime = 10;
    const float chaseRange = 20f;
    Transform player;

    public Vector3 patrolCenter;        // Center position of the patrol range
    public float patrolRadiusY = 3f;    // Radius of the patrol range
    public float patrolRadius = 30f;    // Radius of the patrol range

    private Vector3 targetPosition;     // The current target position for the enemy to move towards
    const float patrolSpeed = 3.5f;

    private Vector3 raycastOrigin;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        timer = 0;
        patrolCenter = animator.transform.position;
        raycastOrigin = animator.transform.position;
        
        GenerateRandomTargetPosition();

    }

    private void GenerateRandomTargetPosition()
    {
        // Perform the raycast downwards
        float depth = 0f;
        if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Terrain"))
                depth = hit.distance;
        }
        // Perform the raycast backwards
        float depthBack = 0f;
        if (Physics.Raycast(raycastOrigin, Vector3.back, out RaycastHit hit2))
        {
            if (hit.collider.CompareTag("Terrain"))
                depthBack = hit2.distance;
        }
        // Perform the raycast frontwards
        float depthFront = 0f;
        if (Physics.Raycast(raycastOrigin, Vector3.forward, out RaycastHit hit3))
        {
            if (hit.collider.CompareTag("Terrain"))
                depthFront = hit3.distance;
        }

        // Generate random positions within the specified range
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        if (depth < randomX)
            randomX = depth;

        float randomY = Random.Range(-patrolRadiusY, patrolRadiusY);
        if (randomY >= 21f)
            randomY = 21f;
     
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        if (randomZ > depthBack)
            randomZ = depthBack;
        else if (randomZ > depthFront)
            randomZ = depthFront;
        // Set the target position based on the random values and the patrol center
        targetPosition = patrolCenter + new Vector3(randomX, randomY, randomZ);
    }

    private void MoveTowardsTargetPosition(Animator animator)
    {
        Vector3 direction = (targetPosition - animator.transform.position);
        direction.Normalize();
        // Implement movement logic here, such as using Translate, Rigidbody, or other movement methods
        // Update the enemy's position based on the desired movement direction and speed
        Vector3 movement = direction * patrolSpeed * Time.deltaTime;
        // Calculate the rotation to face the target position
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        //targetRotation *= Quaternion.Euler(0f, 180f, 0f);
        animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, targetRotation, Time.deltaTime);
        animator.transform.Translate(movement, Space.World);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check if the enemy has reached its target position
        if (Vector3.Distance(animator.transform.position, targetPosition) < 0.5f)
        {
            // In that case, we generate another target position
            GenerateRandomTargetPosition();
        }
        // Independently, move towards the target position
        MoveTowardsTargetPosition(animator);

        // If it has been patrolling for a long time already, change to false
        timer += Time.deltaTime;
        if (timer > patrolTime)
            animator.SetBool("isPatrolling", false);

        // Calculate distance to the player, in case its close, change to Chase State
        float distance = Vector3.Distance(player.position, animator.transform.position);
        if (distance <= chaseRange)
        {
            animator.SetBool("isChasing", true);
            Debug.Log("Leviathan is chasing");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //agent.SetDestination(agent.transform.position);
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
