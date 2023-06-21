using PlayerControls;
using UI.Inventory.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class InventoryMenu : MonoBehaviour
    {
        private Player _player;
        private VisualElement _inventoryContainer;
        private GridInventoryViewer _inventoryViewer;
        private bool _isInventoryMenuOpen;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            _inventoryContainer = _root.Q<VisualElement>("Grid");
            _player = GameObject.FindWithTag("Player").GetComponent<Player>();
        }

        public void Update()
        {
            if (_isInventoryMenuOpen)
            {
                _inventoryViewer.Update();
            }
        }

        public void Rotate(int direction)
        {
            _inventoryViewer?.Rotate(direction);
        }

        public bool IsOpen()
        {
            return _isInventoryMenuOpen;
        }

        private void Open()
        {
            PlayerInventoryViewer inventoryViewer =
                new(_root, _inventoryContainer, _player.GetInventory());
            inventoryViewer.Show();
            _root.style.display = DisplayStyle.Flex;
            _isInventoryMenuOpen = true;
            _inventoryViewer = inventoryViewer;
        }

        private void Close()
        {
            _root.style.display = DisplayStyle.None;
            _isInventoryMenuOpen = false;
            _inventoryViewer.Close();
        }

        public void ToggleMenu()
        {
            if (!_isInventoryMenuOpen)
            {
                Open();
            }
            else
            {
                Close();
            }
        }
    }
}