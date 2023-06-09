using PlayerControls;
using UI.Crafting;
using UI.Inventory;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private CraftingMenu _craftingMenu;
        private InventoryMenu _inventoryMenu;
        private Player _player;
        private PlayerController _playerController;
        private TransferInventoryMenu _transferInventoryMenu;

        private void Start()
        {
            _inventoryMenu = GetComponentInChildren<InventoryMenu>();
            _transferInventoryMenu = GetComponentInChildren<TransferInventoryMenu>();
            _craftingMenu = GetComponentInChildren<CraftingMenu>();

            _player = GameObject.FindWithTag("Player").GetComponent<Player>();
            _playerController = _player.GetComponent<PlayerController>();
        }

        public void Update()
        {
            bool anyOpen = _inventoryMenu.IsOpen() || _transferInventoryMenu.IsOpen() || _craftingMenu.IsOpen();

            if (anyOpen)
            {
                _playerController.LockMovement();
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                _playerController.UnlockMovement();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}