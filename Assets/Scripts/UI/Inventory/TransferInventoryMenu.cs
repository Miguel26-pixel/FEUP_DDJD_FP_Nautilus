using Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class TransferInventoryMenu : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset itemDescriptorTemplate;
        private VisualElement _root;
        private VisualElement _inventoryContainerLeft;
        private VisualElement _inventoryContainerRight;
        private InventoryViewer<InventoryGrid> _inventoryViewerLeft;
        private InventoryViewer<InventoryGrid> _inventoryViewerRight;
        private bool _isTransferOpen;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            _inventoryContainerLeft = _root.Q<VisualElement>("GridLeft");
            _inventoryContainerRight = _root.Q<VisualElement>("GridRight");
        }

        public void Update()
        {
            if (!_isTransferOpen)
            {
                return;
            }

            _inventoryViewerLeft.Update();
            _inventoryViewerRight.Update();
        }

        public void Rotate(int direction)
        {
            _inventoryViewerLeft?.Rotate(direction);
            _inventoryViewerRight?.Rotate(direction);
        }

        private void Open(InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderLeft, InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderRight)
        {
            inventoryViewerBuilderLeft.inventoryContainer = _inventoryContainerLeft;
            inventoryViewerBuilderRight.inventoryContainer = _inventoryContainerRight;
            inventoryViewerBuilderLeft.itemDescriptorTemplate = itemDescriptorTemplate;
            inventoryViewerBuilderRight.itemDescriptorTemplate = itemDescriptorTemplate;
            inventoryViewerBuilderLeft.root = _root;
            inventoryViewerBuilderRight.root = _root;
            
            InventoryViewer<InventoryGrid> inventoryViewerLeft = inventoryViewerBuilderLeft.Build();
            InventoryViewer<InventoryGrid> inventoryViewerRight = inventoryViewerBuilderRight.Build();
            
            inventoryViewerLeft.onDragStart = inventoryViewerRight.HandleDragStart;
            inventoryViewerLeft.onDragEnd = inventoryViewerRight.HandleDragEnd;
            inventoryViewerRight.onDragStart = inventoryViewerLeft.HandleDragStart;
            inventoryViewerRight.onDragEnd = inventoryViewerLeft.HandleDragEnd;
            
            inventoryViewerLeft.Show();
            inventoryViewerRight.Show();
            _root.style.display = DisplayStyle.Flex;
            _isTransferOpen = true;
            _inventoryViewerLeft = inventoryViewerLeft;
            _inventoryViewerRight = inventoryViewerRight;
        }

        public void ToggleMenu(InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderLeft, InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderRight)
        {
            if (!_isTransferOpen)
            {
                Open(inventoryViewerBuilderLeft, inventoryViewerBuilderRight);
            }
            else
            {
                _root.style.display = DisplayStyle.None;
                _isTransferOpen = false;
                _inventoryViewerLeft.Close();
                _inventoryViewerRight.Close();
            }
        }
    }
}