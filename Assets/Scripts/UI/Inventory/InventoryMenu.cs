using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class InventoryMenu : MonoBehaviour
    {
        [SerializeField] private Player.Player player;
        private VisualElement _root;
        private VisualElement _inventoryContainer;
        private bool _isInventoryMenuOpen;
        
        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            _inventoryContainer = _root.Q<VisualElement>("Grid");
        }

        private void Open()
        {
            PlayerInventoryViewer inventoryViewer = new(_inventoryContainer, player.GetInventory());
            inventoryViewer.Show();
            _root.style.display = DisplayStyle.Flex;
            _isInventoryMenuOpen = true;
        }
        
        public void ToggleMenu()
        {
            Debug.Log("hey2");

            if (!_isInventoryMenuOpen)
            {
                Open();
            }
            else
            {
                _root.style.display = DisplayStyle.None;
                _isInventoryMenuOpen = false;
            }
        }
    }
}