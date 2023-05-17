using System;
using Crafting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerTest : MonoBehaviour, PlayerActions.ICraftingTestActions
    {
        private PlayerActions _playerActions;
        public MachineType machineType;
        public UnityEvent<MachineType> OnCraftEvent = new();

        public void OnEnable()
        {
            if (_playerActions == null)
            {
                _playerActions = new PlayerActions();
                _playerActions.CraftingTest.SetCallbacks(this);
            }
            
            _playerActions.CraftingTest.Enable();
        }

        public void OnDisable()
        {
            _playerActions.CraftingTest.Disable();
        }


        public void OnCraft(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            Debug.Log("Craft");
            OnCraftEvent.Invoke(machineType);
        }
    }
}