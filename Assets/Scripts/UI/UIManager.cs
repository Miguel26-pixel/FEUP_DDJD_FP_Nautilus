using UI.Crafting;
using UI.Inventory;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private InventoryMenu _inventoryMenu;
        private TransferInventoryMenu _transferInventoryMenu;
        private CraftingMenu _craftingMenu;
        private Player.Player _player;

        private void Start()
        {
            _inventoryMenu = GetComponentInChildren<InventoryMenu>();
            _transferInventoryMenu = GetComponentInChildren<TransferInventoryMenu>();
            _craftingMenu = GetComponentInChildren<CraftingMenu>();
            
            _player = GameObject.FindWithTag("Player").GetComponent<Player.Player>();
        }
        
        public void Update()
        {
            bool anyOpen = _inventoryMenu.IsOpen() || _transferInventoryMenu.IsOpen() || _craftingMenu.IsOpen();

            if (anyOpen)
            {
                _player.LockMovement();
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                _player.UnlockMovement();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}