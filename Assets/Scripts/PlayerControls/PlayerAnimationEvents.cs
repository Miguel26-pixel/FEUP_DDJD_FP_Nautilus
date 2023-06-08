using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Animator _animator;
    private PlayerController _playerController;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _playerController = GetComponent<PlayerController>();    
    }

    private void Jump()
    {
        _playerController.LiftJump();
    }

    private void Apex()
    {
        _playerController.ApexJump();
    }
    
    private void Land()
    {
    }
}
