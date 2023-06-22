using System;
using System.Linq;
using FMODUnity;
using Inventory;
using Items;
using JetBrains.Annotations;
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

            RegisterFeet();
            RegisterBody1();
            RegisterBody2();
            RegisterHead();
            
            onDragEnd += OnDragEnd;
        }

        private void RegisterFeet()
        {
            _feet.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    ProcessMouseDownLeft(evt, _inventory.feetEquipment, EquipmentSlotType.Feet);
                }
            });

            _feet.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    ProcessContextActionViewer(evt, _inventory.feetEquipment);
                }
            });

            _feet.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (_inventory.feetEquipment is not null) OpenItemInfo(_inventory.feetEquipment);
            });
            _feet.RegisterCallback<MouseLeaveEvent>(_ => CloseItemInfo());
        }
        
        private void RegisterBody1()
        {
            _body1.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    ProcessMouseDownLeft(evt, _inventory.bodyEquipment1, EquipmentSlotType.Feet);
                }
            });

            _body1.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    ProcessContextActionViewer(evt, _inventory.bodyEquipment1);
                }
            });

            _body1.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (_inventory.bodyEquipment1 is not null) OpenItemInfo(_inventory.bodyEquipment1);
            });
            _body1.RegisterCallback<MouseLeaveEvent>(_ => CloseItemInfo());
        }
        private void RegisterBody2()
        {
            _body2.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    ProcessMouseDownLeft(evt, _inventory.bodyEquipment2, EquipmentSlotType.Feet);
                }
            });

            _body2.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    ProcessContextActionViewer(evt, _inventory.bodyEquipment2);
                }
            });

            _body2.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (_inventory.bodyEquipment2 is not null) OpenItemInfo(_inventory.bodyEquipment2);
            });
            _body2.RegisterCallback<MouseLeaveEvent>(_ => CloseItemInfo());
        }
        private void RegisterHead()
        {
            _head.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    ProcessMouseDownLeft(evt, _inventory.headEquipment, EquipmentSlotType.Feet);
                }
            });

            _head.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    ProcessContextActionViewer(evt, _inventory.headEquipment);
                }
            });

            _head.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (_inventory.headEquipment is not null) OpenItemInfo(_inventory.headEquipment);
            });
            _head.RegisterCallback<MouseLeaveEvent>(_ => CloseItemInfo());
        }


        
        private void OnDragEnd(IDraggable draggable)
        {
            if (isDragging)
            {
                // item successfully dropped on another inventory
                if (draggable is not ItemDraggable itemDraggable)
                {
                    return;
                }
                
                var component = draggingProperties.draggedItem.GetComponent<EquipableComponent>().equipableComponentData;

                if (!component.OnUnequip(player, draggingProperties.draggedItem))
                {
                    return;
                }
                isDragging = false;
                draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                _inventory.NotifySubscribersOnInventoryChanged();
            }
            else
            {
                otherDraggable = null;
            }
        }

        private void ProcessMouseDownLeft(MouseDownEvent evt, [CanBeNull] Item item, EquipmentSlotType slot)
        {
            if (item is null)
            {
                return;
            }
            
            draggingProperties = new ItemDraggable(item, slot);

            HandleDragStart(draggingProperties);
            if (_contextMenuViewer.IsOpen)
            {
                return;
            }


            isDragging = true;
            CloseItemInfo();
            RenderItemDrag();
        }
        
        private void ProcessContextActionViewer(EventBase evt, Item item)
        {
            if (isDragging)
            {
                return;
            }

            if (_contextMenuViewer.IsOpen)
            {
                CloseContext();
                return;
            }

            evt.StopPropagation();

            Vector2 position = currentMousePosition;
            CloseItemInfo();
            _contextMenuViewer.Open(item, 0, position, player);
        }

        private void ProcessMouseUpEquipmentSlot(MouseUpEvent evt, EquipmentSlotType equipmentSlotType,
            EquipmentSlot equipmentSlot)
        {
            if (!isDragging)
            {
                return;
            }

            if (draggingProperties.draggedItem.Type != ItemType.Equipment)
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