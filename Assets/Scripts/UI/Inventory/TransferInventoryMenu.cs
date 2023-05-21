using System;
using System.Collections.Generic;
using Inventory;
using UI.Inventory.Builders;
using UI.Inventory.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class TransferInventoryMenu : MonoBehaviour
    {
        [Serializable]
        public class DirectionSprite
        {
            public TransferDirection direction;
            public Sprite sprite;
        }
        
        public DirectionSprite[] transferDirectionArrows;
        
        private readonly Dictionary<TransferDirection, Sprite> _transferDirectionArrows =
            new Dictionary<TransferDirection, Sprite>();

        private VisualElement _root;
        private VisualElement _inventoryContainerLeft;
        private VisualElement _inventoryContainerRight;
        private VisualElement _directionArrow;

        private Label _inventory1Label;
        private Label _inventory2Label;
        
        private InventoryViewer<InventoryGrid> _inventoryViewerLeft;
        private InventoryViewer<InventoryGrid> _inventoryViewerRight;
        
        private bool _isTransferOpen;
        
        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            
            _inventoryContainerLeft = _root.Q<VisualElement>("GridLeft");
            _inventoryContainerRight = _root.Q<VisualElement>("GridRight");
            _directionArrow = _root.Q<VisualElement>("DirectionArrow");

            _inventory1Label = _root.Q<Label>("Inventory1Label");
            _inventory2Label = _root.Q<Label>("Inventory2Label");
            
            foreach (DirectionSprite directionSprite in transferDirectionArrows)
            {
                _transferDirectionArrows.Add(directionSprite.direction, directionSprite.sprite);
            }
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

        private void Open(InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderLeft,
            InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderRight, TransferDirection direction)
        {
            inventoryViewerBuilderLeft.inventoryContainer = _inventoryContainerLeft;
            inventoryViewerBuilderRight.inventoryContainer = _inventoryContainerRight;
            inventoryViewerBuilderLeft.root = _root;
            inventoryViewerBuilderRight.root = _root;

            _inventory1Label.text = inventoryViewerBuilderLeft.inventory.GetInventoryName();
            _inventory2Label.text = inventoryViewerBuilderRight.inventory.GetInventoryName();
            
            _directionArrow.style.backgroundImage = new StyleBackground(_transferDirectionArrows[direction]);

            InventoryViewer<InventoryGrid> inventoryViewerLeft = inventoryViewerBuilderLeft.Build();
            InventoryViewer<InventoryGrid> inventoryViewerRight = inventoryViewerBuilderRight.Build();

            if ((direction & TransferDirection.SourceToDestination) != 0)
            {
                inventoryViewerLeft.onDragStart = inventoryViewerRight.HandleDragStart;
            }
            if ((direction & TransferDirection.DestinationToSource) != 0)
            {
                inventoryViewerRight.onDragStart = inventoryViewerLeft.HandleDragStart;
            }
            
            inventoryViewerLeft.onDragEnd = inventoryViewerRight.HandleDragEnd;
            inventoryViewerRight.onDragEnd = inventoryViewerLeft.HandleDragEnd;

            inventoryViewerLeft.Show();
            inventoryViewerRight.Show();
            _root.style.display = DisplayStyle.Flex;
            _isTransferOpen = true;
            _inventoryViewerLeft = inventoryViewerLeft;
            _inventoryViewerRight = inventoryViewerRight;
        }

        public void ToggleMenu(InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderLeft,
            InventoryViewerBuilder<InventoryGrid> inventoryViewerBuilderRight, TransferDirection direction)
        {
            if (!_isTransferOpen)
            {
                Open(inventoryViewerBuilderLeft, inventoryViewerBuilderRight, direction);
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