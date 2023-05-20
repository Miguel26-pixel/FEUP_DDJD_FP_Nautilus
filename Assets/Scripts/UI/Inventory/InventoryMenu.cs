using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class InventoryMenu : MonoBehaviour
    {
        [SerializeField] private Player.Player player;
        private VisualElement _inventoryContainer;
        private PlayerInventoryViewer _inventoryViewer;
        private bool _isInventoryMenuOpen;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            _inventoryContainer = _root.Q<VisualElement>("Grid");
        }

        public void Update()
        {
            if (_isInventoryMenuOpen)
            {
                _inventoryViewer.Update();
            }
        }

        private void Open()
        {
            PlayerInventoryViewer inventoryViewer = new(_root, _inventoryContainer, player.GetInventory());
            inventoryViewer.Show();
            _root.style.display = DisplayStyle.Flex;
            _isInventoryMenuOpen = true;
            _inventoryViewer = inventoryViewer;
        }

        public void ToggleMenu()
        {
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