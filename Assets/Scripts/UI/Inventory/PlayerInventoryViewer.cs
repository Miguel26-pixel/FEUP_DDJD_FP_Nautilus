using Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class PlayerInventoryViewer : InventoryViewer<PlayerInventory>
    {
        private readonly PlayerInventory _inventory;
        
        public PlayerInventoryViewer(VisualElement inventoryContainer, PlayerInventory inventory) : base(inventoryContainer, inventory)
        {
            _inventory = inventory;
        }

        public override void Show()
        {
            inventoryContainer.Clear();
            for (int row = 0; row < InventoryConstants.PlayerInventoryMaxHeight; row++)
            {
                VisualElement rowElement = new VisualElement();
                rowElement.AddToClassList("row");
                rowElement.name = $"Row{row}";
                
                for (int col = 0; col < InventoryConstants.PlayerInventoryMaxWidth; col++)
                {
                    VisualElement cell = new VisualElement();
                    cell.name = $"Cell{col}";
                    cell.AddToClassList("item-square");
                    
                    VisualElement iconElement = new VisualElement();
                    iconElement.name = "ItemIcon";
                    iconElement.AddToClassList("icon");
                    
                    VisualElement overlayElement = new VisualElement();
                    overlayElement.name = "Overlay";
                    overlayElement.AddToClassList("overlay");
                    
                    cell.Add(iconElement);
                    cell.Add(overlayElement);

                    if (!_inventory.ValidatePosition(new Vector2Int(col, row)))
                    {
                        cell.visible = false;
                    }
                    
                    rowElement.Add(cell);
                }
                
                inventoryContainer.Add(rowElement);
            }
        }
    }
}