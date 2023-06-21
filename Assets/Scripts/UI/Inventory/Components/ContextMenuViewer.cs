using System;
using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.UIElements;
using PlayerControls;

namespace UI.Inventory.Components
{
    public class ContextMenuViewer
    {
        private readonly VisualElement _contextActions;
        private readonly Label _contextTitle;
        private readonly VisualElement _itemContext;

        private readonly Label _noActionsLabel;
        private readonly VisualElement _root;

        private readonly VisualTreeAsset _textButtonTemplate;
        private Item _item;

        public ContextMenuViewer(VisualElement root)
        {
            _root = root;

            _itemContext = root.Q<VisualElement>("ItemContext");
            if (_itemContext == null)
            {
                Resources.Load<VisualTreeAsset>("UI/ItemContext").CloneTree(root);
                _itemContext = root.Q<VisualElement>("ItemContext");
            }

            _contextActions = _itemContext.Q<VisualElement>("ContextActions");

            _textButtonTemplate = Resources.Load<VisualTreeAsset>("UI/TextButton");

            _contextTitle = _itemContext.Q<Label>("ContextTitle");
            _noActionsLabel = _itemContext.Q<Label>("NoActions");

            VisualElement closeContext = _itemContext.Q<VisualElement>("CloseContext");
            closeContext.RegisterCallback<MouseUpEvent>(_ => Close());

            _itemContext.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        public bool IsOpen { get; private set; }
        public uint ItemInfoID { get; private set; }

        public void Open(Item item, uint itemID, Vector2 position, Player player)
        {
            IsOpen = true;
            ItemInfoID = itemID;
            _item = item;

            _contextTitle.text = item.Name;

            List<ContextMenuAction> actions = item.GetContextMenuActions();
            _contextActions.Clear();

            if (actions.Count == 0)
            {
                _noActionsLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                _noActionsLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

                foreach (ContextMenuAction action in actions)
                {
                    VisualElement textButton = _textButtonTemplate.Instantiate();
                    Label label = textButton.Q<Label>("Label");

                    label.text = action.Name;

                    textButton.RegisterCallback<MouseUpEvent>(evt => { ProcessMouseUpAction(evt, action); });

                    _contextActions.Add(textButton);
                }
            }

            _itemContext.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _itemContext.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
            UiUtils.SetTopLeft(position, _itemContext, _root);

            _itemContext.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                UiUtils.SetTopLeft(position, _itemContext, _root);
                UiUtils.MakeVisible(_itemContext);
            });
        }

        public void Close()
        {
            IsOpen = false;
            _itemContext.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void ProcessMouseUpAction(IMouseEvent evt, ContextMenuAction action)
        {
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

            if (player == null) {
                return;
            }

            if (evt.button != 0)
            {
                return;
            }

            try
            {
                action.Action(player, _item);
            }
            catch (NotImplementedException)
            {
                Debug.Log("Action not implemented");
            }

            Close();
        }
    }
}