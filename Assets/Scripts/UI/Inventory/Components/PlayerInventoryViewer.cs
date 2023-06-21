using System;
using System.Linq;
using Inventory;
using Items;
using PlayerControls;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory.Components
{
    public enum EquipmentSlot
    {
        Feet,
        Body1,
        Body2,
        Head
    }
    
    [Serializable]
    public enum EquipmentSlotType
    {
        Feet,
        Body,
        Head
    }
    
    public class PlayerInventoryViewer : GridInventoryViewer, IInventorySubscriber
    {
        private readonly PlayerInventory _inventory;
        private VisualElement _feet;
        private VisualElement _body1;
        private VisualElement _body2;
        private VisualElement _head;

        public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer, PlayerInventory inventory, Player player) :
            base(root, inventoryContainer, inventory, player, refreshAfterMove: false)
        {
            inventory.AddSubscriber(this);
            _inventory = inventory;

            (_feet = root.Q<VisualElement>("Feet")).RegisterCallback<MouseUpEvent>( 
                evt => ProcessMouseUpEquipmentSlot(evt, EquipmentSlotType.Feet, EquipmentSlot.Feet));
            (_body1 = root.Q<VisualElement>("Body1")).RegisterCallback<MouseUpEvent>(
                evt => ProcessMouseUpEquipmentSlot(evt, EquipmentSlotType.Body, EquipmentSlot.Body1));
            (_body2 = root.Q<VisualElement>("Body2")).RegisterCallback<MouseUpEvent>(
                evt => ProcessMouseUpEquipmentSlot(evt, EquipmentSlotType.Body, EquipmentSlot.Body2));
            (_head = root.Q<VisualElement>("Head")).RegisterCallback<MouseUpEvent>(
                evt => ProcessMouseUpEquipmentSlot(evt, EquipmentSlotType.Head, EquipmentSlot.Head));
        }

        private void ProcessMouseUpEquipmentSlot(EventBase evt, EquipmentSlotType equipmentSlotType,
            EquipmentSlot equipmentSlot)
        {
            if (!isDragging)
            {
                return;
            }

            var component = draggingProperties.draggedItem.GetComponent<EquipableComponent>().equipableComponentData;

            if (component.Slot != equipmentSlotType)
            {
                return;
            }
            
            if (!component.OnEquip(player, draggingProperties.draggedItem, equipmentSlot))
            {
                return;
            }

            isDragging = false;
            draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            DarkenItem(_inventory.GetRelativePositionAt(draggingProperties.dragStartPosition.position).itemID,
                false);
                
            if (_inventory.HasItem(draggingProperties.draggedItem.IDHash))
            {
                Debug.Log("Delete has item");
                _inventory.RemoveItem(draggingProperties.draggedItem.IDHash);
                _inventory.NotifySubscribersOnInventoryChanged();
            }
        }

        public void OnInventoryChanged()
        {
            Refresh();

            _feet.Q<VisualElement>("ItemIcon").style.backgroundImage =
                new StyleBackground(_inventory.feetEquipment?.Icon);
            _body1.Q<VisualElement>("ItemIcon").style.backgroundImage =
                new StyleBackground(_inventory.bodyEquipment1?.Icon);
            _body2.Q<VisualElement>("ItemIcon").style.backgroundImage =
                new StyleBackground(_inventory.bodyEquipment2?.Icon);
            _head.Q<VisualElement>("ItemIcon").style.backgroundImage =
                new StyleBackground(_inventory.headEquipment?.Icon);
        }

        public override void Close()
        {
            base.Close();
            _inventory.RemoveSubscriber(this);
        }
    }
}